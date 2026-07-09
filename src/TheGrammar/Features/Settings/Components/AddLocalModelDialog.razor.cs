using DialogResult = MudBlazor.DialogResult;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheGrammar.Features.LocalAI;

namespace TheGrammar.Features.Settings.Components;

public partial class AddLocalModelDialog
{
    private IReadOnlyList<LocalModelInfo> _catalogModels = [];
    private string? _selectedAlias;
    private bool _isLoadingCatalog = true;
    private bool _isDownloading;
    private double _downloadProgress;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] public FoundryLocalCatalogService LocalCatalogService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _catalogModels = await LocalCatalogService.GetCatalogModelsAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load the Foundry Local catalog: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoadingCatalog = false;
        }
    }

    async Task Submit()
    {
        if (string.IsNullOrEmpty(_selectedAlias))
        {
            return;
        }

        _isDownloading = true;
        _downloadProgress = 0;
        StateHasChanged();

        try
        {
            var progress = new Progress<float>(p =>
            {
                _downloadProgress = p;
                InvokeAsync(StateHasChanged);
            });

            await LocalCatalogService.DownloadAndLoadAsync(_selectedAlias, progress);

            SettingsOption.UpdateSettings(nameof(SettingsOption.LocalModelAlias), _selectedAlias);
            MudDialog!.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to download/load model: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isDownloading = false;
            StateHasChanged();
        }
    }

    private void Cancel() => MudDialog.Cancel();
}
