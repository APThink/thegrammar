using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using TheGrammar.Services;

namespace TheGrammar.View.Pages;

public partial class Settings
{
    private string _apiKey = string.Empty;

    [Inject] public IOptionsSnapshot<OpenAiOptions> OpenApiSnapshot { get; set; } = null!;

    [Inject] public ISnackbar Snackbar { get; set; } = null!;

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
            Snackbar.Add("Saved, now you need to restart the app", Severity.Success);
        }
        catch (Exception)
        {
            Snackbar.Add("Failed to save", Severity.Error);
        }
    }
}
