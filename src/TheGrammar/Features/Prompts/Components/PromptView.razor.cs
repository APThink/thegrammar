using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.HotKeys.Services;

namespace TheGrammar.Features.Prompts.Components;

public partial class PromptView
{
    [CascadingParameter] public PromptRepository PromptRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IGlobalKeyBindingState KeyBindingState { get; set; } = null!;
    [Inject] public HotKeyListener HotKeyListener { get; set; } = null!;
    [Parameter] public List<Prompt> Prompts { get; set; } = new List<Prompt>();

    private static List<Keys> PossibleKeys => Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(k => (int)k >= 65 && (int)k <= 90 || (int)k >= 112 && (int)k <= 123).ToList();

    private async Task ChangePromptAsync(Prompt prompt)
    {
        try
        {
            await PromptRepository.UpdatePromptAsync(prompt);
            await KeyBindingState.InitAsync();
            var registered = HotKeyListener.Register(prompt.Id, key: prompt.RightKey, modifiers: prompt.LeftKey, prompt: prompt.Promt);
            if (registered)
            {
                Snackbar.Add($"Updated prompt: {prompt}", Severity.Success);
            }
            else
            {
                Snackbar.Add($"Updated prompt, but the hotkey {prompt} is already in use by another shortcut", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error updating prompt: {ex.Message}", Severity.Error);
        }
    }
}
