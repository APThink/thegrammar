using DialogResult = MudBlazor.DialogResult;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OpenAI;
using TheGrammar.Domain;

namespace TheGrammar.Features.Settings.Components;

public partial class AddModelDialog
{
    private string _key = string.Empty;
    private string _displayName = string.Empty;
    private string _modelName = string.Empty;
    private float? _temperature;
    private float _topP = 1.0f;
    private float _frequencyPenalty = 0.0f;
    private float _presencePenalty = 0.0f;
    private bool _isValidating = false;

    [CascadingParameter] MudDialogInstance? MudDialog { get; set; }
    [Inject] public ModelRepository ModelRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public OpenAIClient OpenAiClient { get; set; } = null!;

    async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(_key) || string.IsNullOrWhiteSpace(_displayName) || string.IsNullOrWhiteSpace(_modelName))
        {
            Snackbar.Add("Key, Display Name and Model Name are required", Severity.Error);
            return;
        }

        _isValidating = true;
        StateHasChanged();

        try
        {
            var modelClient = OpenAiClient.GetOpenAIModelClient();
            await modelClient.GetModelAsync(_modelName.Trim());
        }
        catch (Exception)
        {
            Snackbar.Add($"Model '{_modelName.Trim()}' was not found in OpenAI. Please check the model name.", Severity.Error);
            _isValidating = false;
            StateHasChanged();
            return;
        }

        try
        {
            var model = new Model
            {
                Key = _key.Trim(),
                DisplayName = _displayName.Trim(),
                ModelName = _modelName.Trim(),
                Temperature = _temperature,
                TopP = _topP,
                FrequencyPenalty = _frequencyPenalty,
                PresencePenalty = _presencePenalty
            };

            await ModelRepository.AddModelAsync(model);
            Snackbar.Add($"Model '{model.DisplayName}' added", Severity.Success);
            MudDialog!.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error adding model: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isValidating = false;
            StateHasChanged();
        }
    }

    void Cancel() => MudDialog?.Cancel();
}