using DialogResult = MudBlazor.DialogResult;
using Microsoft.AspNetCore.Components;
using TheGrammar.Services;
using TheGrammar.Data;
using MudBlazor;

namespace TheGrammar.View.Components;

public partial class AddPromptDialog
{
    private Keys _leftKey = Keys.Shift | Keys.Control;
    private Keys _rightKey = Keys.A;
    private string _prompt = "Hello World";

    [CascadingParameter] MudDialogInstance? MudDialog { get; set; }
    [CascadingParameter]  public PromptService PromptService { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;
    [Inject] public KeyBindingState KeyBindingState { get; set; } = null!;

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

            await PromptService.AddPromptAsync(prompt);
            await KeyBindingState.InitAsync();
            Snackbar.Add($"Added prompt: {prompt}", Severity.Success);
            MudDialog!.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error adding prompt: {ex.Message}", Severity.Error);
            MudDialog!.Close(DialogResult.Ok(false));
        }
    }

    void Cancel()
    {
        MudDialog?.Cancel();
    }
}
