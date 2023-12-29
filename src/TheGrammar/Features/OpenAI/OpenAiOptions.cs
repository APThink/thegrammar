using System.Text.Json.Nodes;
using System.Text.Json;

namespace TheGrammar.Features.OpenAI;

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
