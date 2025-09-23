using Microsoft.Extensions.Options;
using Serilog;
using TheGrammar.Domain;

namespace TheGrammar.Features.OpenAI;

public class ChatVersionState
{
  private readonly Dictionary<ChatVersion, ChatModelSettings> _configs;

  public ChatModelSettings Current { get; private set; }

  public ChatVersionState(IOptions<OpenAiOptions> options)
  {
    _configs = new Dictionary<ChatVersion, ChatModelSettings>
    {
      {
        ChatVersion.Gpt5,
        new ChatModelSettings(
          "gpt-5",
          Temperature: null,
          TopP: 1.0f,
          FrequencyPenalty: 0.0f,
          PresencePenalty: 0.0f
        )
      },
      {
        ChatVersion.Gpt5Nano,
        new ChatModelSettings(
          "gpt-5-nano",
          Temperature: null,
          TopP: 1.0f,
          FrequencyPenalty: 0.0f,
          PresencePenalty: 0.0f
        )
      },
      {
        ChatVersion.Turbo4,
        new ChatModelSettings(
          "gpt-4",
          Temperature: 0.0f,
          TopP: 1.0f,
          FrequencyPenalty: 0.0f,
          PresencePenalty: 0.0f
        )
      },
      {
        ChatVersion.Gpt4o,
        new ChatModelSettings(
          "gpt-4o",
          Temperature: 0.0f,
          TopP: 1.0f,
          FrequencyPenalty: 0.0f,
          PresencePenalty: 0.0f
        )
      },
    };
    
    var defaultModel = options.Value.DefaultModel;
    Current = _configs.TryGetValue(defaultModel, out var settings) ? settings : _configs[ChatVersion.Gpt4o];
  }

  public void SetCurrentModel(ChatVersion chatVersion)
  {
    if (_configs.TryGetValue(chatVersion, out var settings))
    {
      Current = settings;
    }
    else
    {
      Log.Error("Failed to set current deployment/model for ChatVersion {ChatVersion}", chatVersion);
    }
  }

  public ChatVersion GetCurrentChatVersion()
  {
    return _configs.FirstOrDefault(x => x.Value == Current).Key;
  }
}
