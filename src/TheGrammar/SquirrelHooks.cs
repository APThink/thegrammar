using Squirrel;
using System.Diagnostics;

namespace TheGrammar;

public class SquirrelHooks
{
    public static void OnAppInstall(SemanticVersion version, IAppTools tools)
    {
        tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
    }

    public static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
    }

    public static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        var processes = Process.GetProcessesByName(processName);

        foreach (var process in processes)
        {
            if (process.Id != Environment.ProcessId)
            {
                process.Kill();
            }
        }

        tools.SetProcessAppUserModelId();
        if (firstRun) MessageBox.Show("Thanks for installing The Grammar!", "Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
