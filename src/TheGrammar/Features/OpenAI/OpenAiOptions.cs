using System.Text.Json;
using System.Text.Json.Nodes;
using TheGrammar.Domain;

namespace TheGrammar.Features.OpenAI;

public class OpenAiOptions
{
  public const string SectionName = "OpenApi";

  public required string ApiKey { get; init; }
  public required int MaxTokenResponse { get; init; }
  public required ChatVersion DefaultModel { get; init; }

  private static string AppSettingsPath =>
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

  private static JsonObject LoadJson()
  {
    if (!File.Exists(AppSettingsPath))
      throw new FileNotFoundException("appsettings.json not found", AppSettingsPath);

    var jsonString = File.ReadAllText(AppSettingsPath);
    return JsonNode.Parse(jsonString) as JsonObject
           ?? throw new Exception("appsettings.json is not a valid json file");
  }

  private static void SaveJson(JsonObject jsonObject)
  {
    var jsonString = jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(AppSettingsPath, jsonString);
  }

  private static JsonObject GetOrCreateSection(JsonObject root)
  {
    root[SectionName] ??= new JsonObject();
    return root[SectionName]!.AsObject();
  }

  public static void UpdateApiKey(string apiKey)
  {
    var root = LoadJson();
    var section = GetOrCreateSection(root);

    section[nameof(ApiKey)] = apiKey;

    SaveJson(root);
  }

  public static void UpdateMaxTokenResponse(int maxTokenResponse)
  {
    var root = LoadJson();
    var section = GetOrCreateSection(root);

    section[nameof(MaxTokenResponse)] = maxTokenResponse;

    SaveJson(root);
  }

  public static void UpdateDefaultModel(ChatVersion model)
  {
    var root = LoadJson();
    var section = GetOrCreateSection(root);
    section[nameof(DefaultModel)] = model.ToString();
    SaveJson(root);
  }
}