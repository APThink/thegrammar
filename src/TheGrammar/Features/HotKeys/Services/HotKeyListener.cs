using System.Runtime.InteropServices;
using System.Diagnostics;
using TheGrammar.Features.HotKeys.Events;

namespace TheGrammar.Features.HotKeys.Services;

public class HotKeyListener
{
    public const int WH_KEYBOARD_LL = 13;
    public const int WM_KEYDOWN = 0x0100;
    public LowLevelKeyboardProc _proc;
    public static nint _hookID = nint.Zero;

    private readonly IGlobalKeyBindingState _keyBindingState;
    private readonly IProcessInputEventService _processInputEventService;

    public HotKeyListener(IGlobalKeyBindingState keyBindingState, IProcessInputEventService processInputEventService)
    {
        _proc = HookCallback;
        _keyBindingState = keyBindingState;
        _processInputEventService = processInputEventService;
    }

    public nint SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    public nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam == WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            var modifiers = Control.ModifierKeys;

            if (_keyBindingState.KeyBindings.TryGetValue((key, modifiers), out string? prompt))
            {
                ModifyClipboardTextAsync(prompt);
                return 1;
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint GetModuleHandle(string lpModuleName);

    public void ModifyClipboardTextAsync(string prompt)
    {
        var userInput = Clipboard.GetText();
        _processInputEventService.TrigerProcessStart(new ProcessStartDto(userInput, prompt));
    }
}
