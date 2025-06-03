using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using TheGrammar.Database;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys.Events;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.Settings;

namespace TheGrammar.Features.HotKeys.Services
{
  public class InputProcessingManager : BackgroundService
  {
    private SettingsOption _settingsOption;
    private readonly IProcessInputEventService _processService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<InputProcessingManager> _logger;
    private readonly OpenAiService _openAiService;
    private readonly CompositeDisposable _subscriptions = new();

    // Track active operations by process ID
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeOperations = new();

    public InputProcessingManager(
      IProcessInputEventService processService,
      OpenAiService openAiService,
      IDbContextFactory<ApplicationDbContext> dbContextFactory,
      ILogger<InputProcessingManager> logger,
      IOptionsMonitor<SettingsOption> settingsMonitor)
    {
      _processService = processService;
      _openAiService = openAiService;
      _dbContextFactory = dbContextFactory;
      _logger = logger;
      _settingsOption = settingsMonitor.CurrentValue;
      settingsMonitor.OnChange(settings => { _settingsOption = settings; });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _subscriptions.Add(_processService.ProcessStartEvents.Subscribe(dto => HandleProcessStart(dto, stoppingToken)));
      _subscriptions.Add(_processService.ProcessCancellationEvents.Subscribe(_ => CancelAllActiveOperations()));
      return Task.CompletedTask;
    }

    private void HandleProcessStart(ProcessStartDto dto, CancellationToken stoppingToken)
    {
      CancelAllActiveOperations();

      var operationId = Guid.NewGuid();
      var operationCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

      _activeOperations[operationId] = operationCts;

      Task.Run(async () =>
      {
        try
        {
          if (_settingsOption.PlaySoundOnProcessStart)
          {
            System.Media.SystemSounds.Exclamation.Play();
          }

          var result = await ProcessUserInputWithCancellationAsync(dto, operationCts.Token);

          // Only proceed if not cancelled
          if (!operationCts.Token.IsCancellationRequested)
          {
            // Update clipboard
            SetClipboardText(result.ModifiedText);

            // Save to database
            await SaveRequest(result).ConfigureAwait(false);

            // Send finish notification
            SendProcessFinishNotification(result);
          }
        }
        catch (OperationCanceledException)
        {
          _logger.LogInformation("Operation was cancelled: {OperationId}", operationId);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error occurred while processing user input");
        }
        finally
        {
          // Clean up by removing from active operations
          _activeOperations.TryRemove(operationId, out _);
          operationCts.Dispose();
        }
      }, stoppingToken);
    }
    
    private async Task<OpenApiResult> ProcessUserInputWithCancellationAsync(ProcessStartDto dto,
      CancellationToken cancellationToken)
    {
      // We need to modify the OpenAiService to accept a cancellation token
      var result = await _openAiService.ProcessAsync(dto.Prompt, dto.Input, cancellationToken).ConfigureAwait(false);
      return result;
    }

    private void CancelAllActiveOperations()
    {
      foreach (var operation in _activeOperations)
      {
        try
        {
          if (!operation.Value.IsCancellationRequested)
          {
            operation.Value.Cancel();
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error cancelling operation {OperationId}", operation.Key);
        }
      }
    }

    private void SetClipboardText(string modifiedText)
    {
      var staThread = new Thread(() =>
      {
        try
        {
          if (_settingsOption.AddAsteriskAtTheEndOfResponse)
          {
            modifiedText += " *";
          }

          Clipboard.SetText(modifiedText);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error occurred while setting clipboard text");
        }
      });
      staThread.SetApartmentState(ApartmentState.STA);
      staThread.Start();
      staThread.Join();
    }

    private async Task SaveRequest(OpenApiResult openApiResult)
    {
      using var context = _dbContextFactory.CreateDbContext();
      var request = new Request
      {
        RequestText = openApiResult.OriginalText,
        ResponseText = openApiResult.ModifiedText,
        ChatVersion = openApiResult.ChatVersion
      };
      context.Requests.Add(request);
      await context.SaveChangesAsync();
    }

    private void SendProcessFinishNotification(OpenApiResult openApiResult)
    {
      var processFinishDto = new ProcessFinishDto(
        OriginalText: openApiResult.OriginalText,
        ModifiedText: openApiResult.ModifiedText,
        ChatVersion: openApiResult.ChatVersion);
      _processService.TriggerProcessFinish(processFinishDto);
    }

    public override void Dispose()
    {
      CancelAllActiveOperations();
      foreach (var operation in _activeOperations.Values)
        operation.Dispose();

      _activeOperations.Clear();
      _subscriptions.Dispose();
      base.Dispose();
    }
  }
}