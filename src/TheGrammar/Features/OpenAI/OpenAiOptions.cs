using System.Text.Json.Nodes;
using System.Text.Json;

namespace TheGrammar.Features.OpenAI;

public class OpenAiOptions
{
  public const string SectionName = "OpenApi";
  public required string ApiKey { get; init; }
  public required int MaxTokenResponse { get; init; }

  public static void UpdateApiKey(string apiKey)
  {
    var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    var jsonString = File.ReadAllText(appSettingsPath);
    var jsonObject = JsonNode.Parse(jsonString) as JsonObject ??
                     throw new Exception("appsettings.json is not a valid json file");

    jsonObject[SectionName] ??= new JsonObject();

    var apiKeyNode = jsonObject[SectionName] ?? throw new Exception("appsettings.json is not contain openapi section");
    apiKeyNode[nameof(ApiKey)] = apiKey;

    jsonString = jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(appSettingsPath, jsonString);
  }
}