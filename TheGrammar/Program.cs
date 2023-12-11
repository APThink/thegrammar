namespace TheGrammar;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var hookID = GlobalHotKeyHandler.SetHook(GlobalHotKeyHandler._proc);
        GlobalHotKeyHandler._hookID = hookID;
        Application.Run(new Main());
        GlobalHotKeyHandler.UnhookWindowsHookEx(hookID);
    }
}