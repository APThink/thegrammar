using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Data;
using Serilog;

namespace TheGrammar.Services;

public class GlobalHotKeyService
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    public LowLevelKeyboardProc _proc;
    public static IntPtr _hookID = IntPtr.Zero;

    private readonly OpenAiService _grammarProcessor;
    private readonly PushService _eventService;
    private readonly KeyBindingState _keyBindingState;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public GlobalHotKeyService(OpenAiService grammarProcessor, PushService eventService, KeyBindingState keyBindingState, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _proc = HookCallback;
        _grammarProcessor = grammarProcessor;
        _eventService = eventService;
        _keyBindingState = keyBindingState;
        _dbContextFactory = dbContextFactory;
    }

    public IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            var modifiers = Control.ModifierKeys;

            if (_keyBindingState.KeyBindings.TryGetValue((key, modifiers), out string? prompt))
            {
                ModifyClipboardTextAsync(prompt);
                return (IntPtr)1;
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

    public void ModifyClipboardTextAsync(string prompt)
    {
        try
        {
            var staThread = new Thread(() =>
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var modText = _grammarProcessor.Process(prompt, text).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (string.IsNullOrEmpty(modText))
                    {
                        return;
                    }

                    SaveRequest(text, modText).ConfigureAwait(false).GetAwaiter().GetResult();
                    SendPush(text, modText);

                    Clipboard.SetText(modText);
                    new ToastContentBuilder().AddText("Text modified and copied to clipboard").Show();
                    return;
                }

                Log.Warning("Clipboard does not contain text");
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

        }
        catch (Exception ex)
        {
            Log.Error($"Failed to process: {ex.Message}");
        }
    }

    private async Task SaveRequest(string text, string modifiedText)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var request = new Request
        {
            RequestText = text,
            ResponseText = modifiedText,
        };

        context.Requests.Add(request);
        await context.SaveChangesAsync();
    }

    private void SendPush(string text, string modifiedText)
    {
        var pushDto = new PushDto
        {
            ModifiedText = modifiedText,
            Text = text
        };

        _eventService.TriggerEvent(pushDto);
    }
}
