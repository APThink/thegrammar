using OpenAI;
using OpenAI.Chat;
using Serilog;

namespace TheGrammar.Features.OpenAI;

public class OpenAiService
{
  private readonly OpenAIClient _openAiClient;
  private readonly ChatVersionState _chatVersionState;

  public OpenAiService(OpenAIClient openAiClient, ChatVersionState chatVersionState)
  {
    _openAiClient = openAiClient;
    _chatVersionState = chatVersionState;
  }

  public async Task<OpenApiResult> ProcessAsync(string systemPrompt, string userInput,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userInput)
      };

      var chatModelSettings = _chatVersionState.Current;
      var options = MapToOptions(chatModelSettings);

      var chatClient = _openAiClient.GetChatClient(chatModelSettings.ModelName);
      var response = await chatClient.CompleteChatAsync(messages, options, cancellationToken);

      var modified = response.Value.Content
        .Where(c => !string.IsNullOrWhiteSpace(c.Text))
        .Select(c => c.Text.Trim())
        .LastOrDefault() ?? userInput;

      return new OpenApiResult(userInput, modified, _chatVersionState.GetCurrentChatVersion());
    }
    catch (TaskCanceledException)
    {
      Log.Warning("Chat request was canceled");
      throw;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Failed to get response from OpenAI chat");
      throw;
    }
  }

  private static ChatCompletionOptions MapToOptions(ChatModelSettings settings)
  {
    var options = new ChatCompletionOptions
    {
      TopP = settings.TopP,
      FrequencyPenalty = settings.FrequencyPenalty,
      PresencePenalty = settings.PresencePenalty,
      ResponseFormat = ChatResponseFormat.CreateTextFormat()
    };

    if (settings.Temperature.HasValue)
      options.Temperature = settings.Temperature.Value;

    return options;
  }
}