using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.Settings.Components;
using MudBlazor;

namespace TheGrammar.Features.Settings;

public partial class Settings
{
  private string _apiKey = string.Empty;
  private SettingsOption _settingsOption = new();

  [Inject] public IOptionsSnapshot<OpenAiOptions> OpenApiSnapshot { get; set; } = null!;
  [Inject] public IOptionsSnapshot<SettingsOption> SettingsOptionSnapshot { get; set; } = null!;
  [Inject] public ISnackbar Snackbar { get; set; } = null!;
  [Inject] public ChatVersionState ChatVersionState { get; set; } = null!;
  [Inject] public IDialogService DialogService { get; set; } = null!;
  [Inject] public ModelRepository ModelRepository { get; set; } = null!;

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

  public async Task AddModel()
  {
    var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
    var dialogReference = DialogService.Show<AddModelDialog>("Add New Model", options);
    var result = await dialogReference.Result;

    if (result is { Canceled: false, Data: true })
    {
      StateHasChanged();
    }
  }

  public void OnSelectedModelChanged(string modelKey)
  {
    ChatVersionState.SetCurrentModel(modelKey);
    OpenAiOptions.UpdateDefaultModel(modelKey);
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
    SettingsOption.UpdateSettings(nameof(SettingsOption.AddAsteriskAtTheEndOfResponse),
      shouldAddAsteriskAtTheEndOfResponse);
  }
}