using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using TheGrammar.Features.OpenAI;
using TheGrammar.Domain;
using MudBlazor;

namespace TheGrammar.Features.Settings;

public partial class Settings
{
    private string _apiKey = string.Empty;

    [Inject] public IOptionsSnapshot<OpenAiOptions> OpenApiSnapshot { get; set; } = null!;

    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    [Inject] public ChatVersionState ChatVersionState { get; set; } = null!;


    protected override void OnInitialized()
    {
        _apiKey = OpenApiSnapshot.Value.ApiKey;
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
}
