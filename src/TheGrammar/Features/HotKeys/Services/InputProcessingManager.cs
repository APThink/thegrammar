using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Linq;
using TheGrammar.Database;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys.Events;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.Settings;

namespace TheGrammar.Features.HotKeys.Services;

public class InputProcessingManager : BackgroundService
{
    private SettingsOption _settingsOption;

    private readonly IProcessInputEventService _processService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<InputProcessingManager> _logger;

    private readonly OpenAiService _openAiService;

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
        settingsMonitor.OnChange(settings =>
        {
            _settingsOption = settings;
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processService.ProcessStartEvents.SelectMany(ProccessTheUserInputAsync).Subscribe(
        async result =>
        {
            SetClipboardText(result.ModifiedText);
            await SaveRequest(result).ConfigureAwait(false);
            SendProcessFinishNotification(result);
        },
        error =>
        {
            _logger.LogError(error, "Error occured while processing user input");
        });

        return Task.CompletedTask;
    }

    private async Task<OpenApiResult> ProccessTheUserInputAsync(ProcessStartDto pushDto)
    {
        if (_settingsOption.PlaySoundOnProcessStart)
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        var result = await _openAiService.ProcessAsync(pushDto.Prompt, pushDto.Input).ConfigureAwait(false);
        return result;
    }

    private void SetClipboardText(string modifiedText)
    {
        var staThread = new Thread(() =>
        {
            try
            {
                if(_settingsOption.AddAsteriskAtTheEndOfResponse)
                {
                    modifiedText += " *";
                }

                Clipboard.SetText(modifiedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while setting clipboard text");
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

        _processService.TrigerProcessFinish(processFinishDto);
    }
}