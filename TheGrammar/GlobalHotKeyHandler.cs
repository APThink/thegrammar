using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TheGrammar;

public class GlobalHotKeyHandler
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    public static LowLevelKeyboardProc _proc = HookCallback;
    public static IntPtr _hookID = IntPtr.Zero;

    public static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if ((Keys)vkCode == Keys.Y && Control.ModifierKeys == (Keys.Control | Keys.Shift))
            {
                ModifyClipboardTextAsync();
                return (IntPtr)1; // Eat keystroke
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    public static void ModifyClipboardTextAsync()
    {
        try
        {
            var staThread = new Thread(() =>
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var processor = new GrammarProcessor();
                    var modText = processor.Process(text).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (string.IsNullOrEmpty(modText))
                    {
                        return;
                    }

                    Clipboard.SetText(modText);
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            new ToastContentBuilder().AddText("Text modified and copied to clipboard").Show();
        }
        catch (Exception ex)
        {
            new ToastContentBuilder().AddText($"Error modifying text: {ex.Message}").Show();
        }
    }
}
