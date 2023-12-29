using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheGrammar.Domain;

namespace TheGrammar.Features.Prompts.Components;

public partial class PromptView
{
    [CascadingParameter] public PromptRepository PromptRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Parameter] public List<Prompt> Prompts { get; set; } = new List<Prompt>();

    private static List<Keys> PossibleKeys => Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(k => (int)k >= 65 && (int)k <= 90 || (int)k >= 112 && (int)k <= 123).ToList();

    private async Task ChangePromptAsync(Prompt prompt)
    {
        try
        {
            await PromptRepository.UpdatePromptAsync(prompt);
            Snackbar.Add($"Updated prompt: {prompt}", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error updating prompt: {ex.Message}", Severity.Error);
        }
    }
}
