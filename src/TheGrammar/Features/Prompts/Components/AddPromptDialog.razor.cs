using DialogResult = MudBlazor.DialogResult;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.HotKeys.Services;
using TheGrammar.Domain;

namespace TheGrammar.Features.Prompts.Components;

public partial class AddPromptDialog
{
    private Keys _leftKey = Keys.Shift | Keys.Control;
    private Keys _rightKey = Keys.A;
    private string _prompt = "Hello World";

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject]  public PromptRepository PromptRepository { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public IGlobalKeyBindingState KeyBindingState { get; set; } = null!;
    [Inject] public HotKeyListener HotKeyListener { get; set; } = null!;

    private static List<Keys> PossibleKeys => Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(k => (int)k >= 65 && (int)k <= 90 || (int)k >= 112 && (int)k <= 123).ToList();
    async Task Submit()
    {
        try
        {
            var prompt = new Prompt
            {
                LeftKey = _leftKey,
                RightKey = _rightKey,
                Promt = _prompt
            };

            await PromptRepository.AddPromptAsync(prompt);
            await KeyBindingState.InitAsync();
            var registered = HotKeyListener.Register(prompt.Id, key: prompt.RightKey, modifiers: prompt.LeftKey, prompt: prompt.Promt);
            if (registered)
            {
                Snackbar.Add($"Added prompt: {prompt}", Severity.Success);
            }
            else
            {
                Snackbar.Add($"Added prompt, but the hotkey {prompt} is already in use by another shortcut", Severity.Warning);
            }
            MudDialog!.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error adding prompt: {ex.Message}", Severity.Error);
            MudDialog!.Close(DialogResult.Ok(false));
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
