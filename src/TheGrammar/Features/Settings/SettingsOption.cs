using System.Text.Json.Nodes;
using System.Text.Json;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.PrompProcessor;

namespace TheGrammar.Features.Settings;

public class SettingsOption
{
    public const string SectionName = "SettingsOption";

    public bool AutoStartEnabled { get; set; }
    public bool PlaySoundOnProcessStart { get; set; }
    public bool AddAsteriskAtTheEndOfResponse { get; set; }
    public AiProvider AiProvider { get; set; } = AiProvider.OpenAi;
    public string? LocalModelAlias { get; set; }

    public static void UpdateSettings(string propertyName, bool value) => UpdateSettingsInternal(propertyName, value);

    public static void UpdateSettings(string propertyName, string value) => UpdateSettingsInternal(propertyName, value);

    private static void UpdateSettingsInternal(string propertyName, JsonNode? value)
    {
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        var jsonString = File.ReadAllText(appSettingsPath);
        var jsonObject = JsonNode.Parse(jsonString) as JsonObject ?? throw new Exception("appsettings.json is not a valid json file");

        if (jsonObject[SectionName] == null)
        {
            jsonObject[SectionName] = new JsonObject();
        }

        var settingsNode = jsonObject[SectionName] as JsonObject ?? throw new Exception($"appsettings.json does not contain the {SectionName} section");

        settingsNode[propertyName] = value;

        jsonString = jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(appSettingsPath, jsonString);
    }
}
