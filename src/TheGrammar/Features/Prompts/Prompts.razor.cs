using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys.Services;
using TheGrammar.Features.Prompts.Components;

namespace TheGrammar.Features.Prompts;

public partial class Prompts
{
    [Inject] public IDialogService DialogService { get; set; } = null!;
    [Inject] public PromptRepository PromptRepository { get; set; } = null!;
    [Inject] public HotKeyListener HotKeyListener { get; set; } = null!;

    private List<Prompt> prompts = new();
    protected override async Task OnInitializedAsync()
    {
        await LoadPrompts();
    }

    private async Task LoadPrompts()
    {
        prompts = await PromptRepository.GetPromptsAsync();
    }

    private async Task AddPrompt()
    {
        var parameters = new DialogParameters<AddPromptDialog> { };
        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };
        var dialogReference = await DialogService.ShowAsync<AddPromptDialog>("Add New Prompt", parameters, options);

        var result = await dialogReference.Result;

        if (result.Data is bool success && success)
        {
            await LoadPrompts();
        }
        
        HotKeyListener.Refresh();
    }
}
