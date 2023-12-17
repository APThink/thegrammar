using Microsoft.AspNetCore.Components;
using TheGrammar.View.Components;
using TheGrammar.Data;
using MudBlazor;
using TheGrammar.Services;

namespace TheGrammar.View.Pages;

public partial class Prompts
{
    [Inject] public IDialogService DialogService { get; set; } = null!;
    [Inject] public PromptService PromptService { get; set; } = null!;

    private List<Prompt> prompts = new();
    protected override async Task OnInitializedAsync()
    {
        await LoadPrompts();
    }

    private async Task LoadPrompts()
    {
        prompts = await PromptService.GetPromptsAsync();
    }

    private async Task AddPrompt()
    {
        var parameters = new DialogParameters<AddPromptDialog> { };
        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Small };
        DialogService.Show<AddPromptDialog>("Add New Prompt", parameters, options);
        await LoadPrompts();
    }
}
