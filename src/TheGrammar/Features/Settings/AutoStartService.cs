using Microsoft.Win32;

namespace TheGrammar.Features.Settings;

public class AutoStartService
{
    public static bool SetAutoStart(string appName, string appPath)
    {
        var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        
        if (key != null)
        {
            key.SetValue(appName, $"\"{appPath}\"");
            return true;
        }

        return false;
    }

    public static bool RemoveAutoStart(string appName)
    {
        var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        if (key != null)
        {
            key.DeleteValue(appName, false);
            return true;
        }

        return false;
    }
}
