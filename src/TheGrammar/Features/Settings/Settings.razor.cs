using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.Settings.Components;
using MudBlazor;

namespace TheGrammar.Features.Settings;

public partial class Settings
{
  private string _apiKey = string.Empty;
  private string _newApiKey = string.Empty;
  private bool _isEditingApiKey;
  private SettingsOption _settingsOption = new();

  private string MaskedApiKey => _apiKey.Length > 4
    ? new string('•', _apiKey.Length - 4) + _apiKey[^4..]
    : new string('•', _apiKey.Length);

  public void StartEditApiKey()
  {
    _newApiKey = string.Empty;
    _isEditingApiKey = true;
  }

  public void CancelEditApiKey()
  {
    _isEditingApiKey = false;
    _newApiKey = string.Empty;
  }

  [Inject] public IOptionsSnapshot<OpenAiOptions> OpenApiSnapshot { get; set; } = null!;
  [Inject] public IOptionsSnapshot<SettingsOption> SettingsOptionSnapshot { get; set; } = null!;
  [Inject] public ISnackbar Snackbar { get; set; } = null!;
  [Inject] public ChatVersionState ChatVersionState { get; set; } = null!;
  [Inject] public IDialogService DialogService { get; set; } = null!;
  [Inject] public ModelRepository ModelRepository { get; set; } = null!;

  protected override void OnInitialized()
  {
    _apiKey = ApiKeyStore.Load() ?? string.Empty;
    _settingsOption = SettingsOptionSnapshot.Value;

    StateHasChanged();
    base.OnInitialized();
  }

  public void SaveButtonClickHandler()
  {
    Snackbar.Add("Saved", Severity.Success);
  }

  public async Task SaveOpenAiClickHandler()
  {
    try
    {
      if (!_isEditingApiKey)
      {
        Snackbar.Add("Saved", Severity.Success);
        return;
      }

      if (string.IsNullOrWhiteSpace(_newApiKey))
      {
        Snackbar.Add("Api key is required", Severity.Error);
        return;
      }

      ApiKeyStore.Save(_newApiKey);
      Snackbar.Add("Saved. Restarting...", Severity.Success);
      StateHasChanged();

      await Task.Delay(1500);
      Application.Restart();
    }
    catch (Exception)
    {
      Snackbar.Add("Failed to save", Severity.Error);
    }
  }

  public async Task AddModel()
  {
    var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
    var dialogReference = await DialogService.ShowAsync<AddModelDialog>("Add New Model", options);
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