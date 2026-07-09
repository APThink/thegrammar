using Microsoft.Extensions.Options;
using TheGrammar.Features.LocalAI;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.Settings;

namespace TheGrammar.Features.PrompProcessor;

public interface IAiService
{
  Task<AiResult> ProcessAsync(string prompt, string input, CancellationToken ct = default);
}

public record AiResult(
  string OriginalText,
  string ModifiedText,
  string ModelKey);
  
public enum AiProvider
{
  OpenAi,
  Local
}

public class AiServiceResolver
{
  private readonly IAiService _openAiService;
  private readonly IAiService _localAiService;
  private readonly IOptionsMonitor<SettingsOption> _settingsMonitor;

  public AiServiceResolver(
    OpenAiServiceAdapter openAiAdapter,
    FoundryLocalAiService foundryLocalService,
    IOptionsMonitor<SettingsOption> settingsMonitor)
    : this((IAiService)openAiAdapter, (IAiService)foundryLocalService, settingsMonitor)
  {
  }

  internal AiServiceResolver(IAiService openAiService, IAiService localAiService, IOptionsMonitor<SettingsOption> settingsMonitor)
  {
    _openAiService = openAiService;
    _localAiService = localAiService;
    _settingsMonitor = settingsMonitor;
  }

  public IAiService GetCurrentService()
  {
    return _settingsMonitor.CurrentValue.AiProvider == AiProvider.Local
      ? _localAiService
      : _openAiService;
  }
}