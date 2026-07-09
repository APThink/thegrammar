using System.Text.RegularExpressions;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using TheGrammar.Features.PrompProcessor;

namespace TheGrammar.Features.LocalAI;

public class FoundryLocalAiService(FoundryLocalCatalogService catalogService) : IAiService
{
  private static readonly Regex ThinkTagsRegex = new(@"<think>[\s\S]*?</think>", RegexOptions.Compiled);

  public async Task<AiResult> ProcessAsync(string prompt, string input, CancellationToken ct = default)
  {
    var session = await catalogService.GetChatClientForCurrentModelAsync(ct);

    var response = await session.ChatClient.CompleteChatAsync([
      new ChatMessage { Role = "system", Content = prompt },
      new ChatMessage { Role = "user", Content = input + " /no_think" }
    ], ct);

    var content = response.Choices?[0].Message.Content ?? string.Empty;
    var cleanContent = ThinkTagsRegex.Replace(content, "").Trim();

    return new AiResult(input, cleanContent, session.Alias);
  }
}
