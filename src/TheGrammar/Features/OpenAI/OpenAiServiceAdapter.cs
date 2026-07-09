using TheGrammar.Features.HotKeys;
using TheGrammar.Features.PrompProcessor;

namespace TheGrammar.Features.OpenAI;

public class OpenAiServiceAdapter(OpenAiService openAiService) : IAiService
{
  public async Task<AiResult> ProcessAsync(string prompt, string input, CancellationToken ct = default)
  {
    var result = await openAiService.ProcessAsync(prompt, input, ct);
    return new AiResult(result.OriginalText, result.ModifiedText, result.ModelKey);
  }
}