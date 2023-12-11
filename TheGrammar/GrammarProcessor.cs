using OpenAI_API.Completions;
using OpenAI_API;

namespace TheGrammar;

public class GrammarProcessor
{
    public async Task<string> Process(string input)
    {
        try
        {
            var outputResult = "";
            var openai = new OpenAIAPI(Properties.Settings.Default.ApiKey);
            var completionRequest = new CompletionRequest();

            var query = $"{Properties.Settings.Default.Prompt} {input}";

            completionRequest.Prompt = query;
            completionRequest.MaxTokens = 2000;

            var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

            foreach (var completion in completions.Completions)
            {
                outputResult += completion.Text;
            }

            return $"{outputResult.Trim()} *";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
