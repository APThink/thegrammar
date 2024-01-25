using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using TheGrammar.Features.OpenAI;
using TheGrammar.Domain;
using MudBlazor;

namespace TheGrammar.Features.Settings;

public partial class Settings
{
    private string _apiKey = string.Empty;
    private SettingsOption _settingsOption = new();
    public bool Label_Switch3 { get; set; } = true;

    [Inject] public IOptionsSnapshot<OpenAiOptions> OpenApiSnapshot { get; set; } = null!;
    [Inject] public IOptionsSnapshot<SettingsOption> SettingsOptionSnapshot { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public ChatVersionState ChatVersionState { get; set; } = null!;

    protected override void OnInitialized()
    {
        _apiKey = OpenApiSnapshot.Value.ApiKey;
        _settingsOption = SettingsOptionSnapshot.Value;

        StateHasChanged();
        base.OnInitialized();
    }

    public void SaveButtonClickHandler()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Snackbar.Add("Api key is required", Severity.Error);
                return;
            }

            OpenAiOptions.UpdateApiKey(_apiKey);
            StateHasChanged();
            Snackbar.Add("Saved, if your changed the api key you need to restart the app", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to save", Severity.Error);
        }
    }

    public void OnSelectedChatVersionChanged(ChatVersion chatVersion)
    {
        ChatVersionState.SetCurrentModel(chatVersion);
    }
    public void OnAutoStartChanged(bool shouldAutoStart)
    {
        _settingsOption.AutoStartEnabled = shouldAutoStart;
        SettingsOption.UpdateSettings(nameof(SettingsOption.AutoStartEnabled), shouldAutoStart);   
    }

    public void OnPlaySoundOnProcessStartChanged(bool shouldPlaySoundOnProcessStart)
    {
        _settingsOption.PlaySoundOnProcessStart = shouldPlaySoundOnProcessStart;
        SettingsOption.UpdateSettings(nameof(SettingsOption.PlaySoundOnProcessStart), shouldPlaySoundOnProcessStart);
    }

    public void OnAddAsteriskAtTheEndOfResponseChanged(bool shouldAddAsteriskAtTheEndOfResponse)
    {
        _settingsOption.AddAsteriskAtTheEndOfResponse = shouldAddAsteriskAtTheEndOfResponse;
        SettingsOption.UpdateSettings(nameof(SettingsOption.AddAsteriskAtTheEndOfResponse), shouldAddAsteriskAtTheEndOfResponse);
    }
}
