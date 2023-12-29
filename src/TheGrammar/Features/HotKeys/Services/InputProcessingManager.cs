using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using TheGrammar.Database;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys.Events;
using TheGrammar.Features.OpenAI;

namespace TheGrammar.Features.HotKeys.Services;

public class InputProcessingManager : BackgroundService
{
    private readonly IProcessInputEventService _processService;
    private readonly OpenAiService _openAiService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly HotKeyPressedNotification _pushService;
    private readonly ILogger<InputProcessingManager> _logger;

    public InputProcessingManager(IProcessInputEventService processService,
        OpenAiService openAiService,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        HotKeyPressedNotification pushService, ILogger<InputProcessingManager> logger)
    {
        _processService = processService;
        _openAiService = openAiService;
        _dbContextFactory = dbContextFactory;
        _pushService = pushService;
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processService.Events.SelectMany(ProccessTheUserInputAsync).Subscribe(
        async result =>
        {
            SetClipboardText(result.ModifiedText);
            await SaveRequest(result).ConfigureAwait(false);
            SendUserInputProcessedPushNotification(result);
        },
        error =>
        {
            _logger.LogError(error, "Error occured while processing user input");
        });

        return Task.CompletedTask;
    }

    private async Task<OpenApiResult> ProccessTheUserInputAsync(UserInput pushDto)
    {
        System.Media.SystemSounds.Exclamation.Play();
        var result = await _openAiService.ProcessAsync(pushDto.Prompt, pushDto.Input).ConfigureAwait(false);
        return result;
    }

    private void SetClipboardText(string modifiedText)
    {
        var staThread = new Thread(() =>
        {
            try
            {
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

    private void SendUserInputProcessedPushNotification(OpenApiResult openApiResult)
    {
        var pushDto = new HotKeyPressedNotificationDto
        {
            OriginalText = openApiResult.OriginalText,
            ModifiedText = openApiResult.ModifiedText,
            ChatVersion = openApiResult.ChatVersion
        };

        _pushService.TriggerEvent(pushDto);
    }
}