using OpenAI_API.Completions;
using OpenAI_API;
using System.Text.Json.Nodes;
using System.Text.Json;
using Serilog;

namespace TheGrammar.Services;

public class OpenAiService
{
    private readonly IOpenAIAPI _openAIAPI;

    public OpenAiService(IOpenAIAPI openAIAPI)
    {
        _openAIAPI = openAIAPI;
    }
    

    public async Task<string> Process(string promt, string input)
    {
        try
        {
            var outputResult = "";

            var completionRequest = new CompletionRequest();

            var query = $"{promt} {input}";

            completionRequest.Prompt = query;
            completionRequest.MaxTokens = 2000;

            var completions = await _openAIAPI.Completions.CreateCompletionAsync(completionRequest);

            foreach (var completion in completions.Completions)
            {
                outputResult += completion.Text;
            }

            return $"{outputResult.Trim()} *";
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to process: {ex.Message}");
            throw;
        }
        
    }
}


public class OpenAiOptions
{
    public const string SectionName = "OpenApi";
    public required string ApiKey { get; set; }
    public required int MaxTokenResponse { get; set; }

    public static void UpdateApiKey(string apiKey)
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var jsonString = File.ReadAllText(appSettingsPath);
        var jsonObject = JsonNode.Parse(jsonString) as JsonObject ?? throw new Exception("appsettings.json is not a valid json file");

        if (jsonObject[SectionName] == null)
        {
            jsonObject[SectionName] = new JsonObject();
        }

        var apiKeyNode = jsonObject[SectionName] ?? throw new Exception("appsettings.json is not contain openapi section");
        apiKeyNode[nameof(ApiKey)] = apiKey;

        jsonString = jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(appSettingsPath, jsonString);
    }
}
