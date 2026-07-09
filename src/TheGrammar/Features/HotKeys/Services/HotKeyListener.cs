using System.Runtime.InteropServices;
using Serilog;
using TheGrammar.Features.HotKeys.Events;

namespace TheGrammar.Features.HotKeys.Services
{
    public class HotKeyListener : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int WM_INPUTLANGCHANGE = 0x0051;

        [Flags]
        private enum FsModifiers : uint
        {
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            NoRepeat = 0x4000
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        private readonly IGlobalKeyBindingState _keyBindingState;
        private readonly IProcessInputEventService _processInputEventService;

        // promptId -> the OS hotkey id currently registered for it
        private readonly Dictionary<int, int> _promptIdToHotkeyId = new();
        // promptId -> the key combination currently registered for it, so Register can tell
        // whether a re-registration actually changes the combination or is just a text edit
        private readonly Dictionary<int, (Keys Key, Keys Modifiers)> _promptIdToBinding = new();
        // OS hotkey id -> prompt text, used to resolve WM_HOTKEY messages
        private readonly Dictionary<int, string> _idToPrompt = new();
        private int _nextId = 1;
        private bool _disposed;

        // Exposed so tests can force a clipboard read failure without relying on the real,
        // hard-to-simulate CLIPBRD_E_CANT_OPEN condition.
        internal Func<string> ClipboardTextProvider { get; set; } = Clipboard.GetText;

        public HotKeyListener(
            IGlobalKeyBindingState keyBindingState,
            IProcessInputEventService processInputEventService)
        {
            _keyBindingState = keyBindingState;
            _processInputEventService = processInputEventService;

            var cp = new CreateParams { Caption = "TheGrammarHotKeyWindow" };
            CreateHandle(cp);
        }

        public void RegisterAll() => RegisterAll(withRetry: true);

        private void RegisterAll(bool withRetry)
        {
            ThrowIfDisposed();

            if (!_keyBindingState.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(GlobalKeyBindingState)}.{nameof(GlobalKeyBindingState.InitAsync)}() must complete before {nameof(RegisterAll)}() is called, otherwise hotkeys would silently register as empty.");
            }

            foreach (var (promptId, binding) in _keyBindingState.KeyBindings)
            {
                Register(promptId, key: binding.RightKey, modifiers: binding.LeftKey, prompt: binding.Prompt, withRetry: withRetry);
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _idToPrompt.Keys)
            {
                UnregisterHotKey(Handle, id);
            }
            _idToPrompt.Clear();
            _promptIdToHotkeyId.Clear();
            _promptIdToBinding.Clear();
        }

        public bool Register(int promptId, Keys key, Keys modifiers, string prompt) =>
            Register(promptId, key, modifiers, prompt, withRetry: true);

        // Registers (or replaces) the hotkey for a single prompt without touching any other
        // currently-registered hotkey. The new combination is registered before the old one is
        // torn down, so a failed re-registration leaves the previous, working hotkey intact
        // instead of dropping the prompt's hotkey entirely. If the combination is unchanged from
        // what's already registered (e.g. only the prompt text was edited), the OS registration
        // is left untouched entirely - re-registering the same combination under a new id would
        // fail, since the old id still owns it.
        private bool Register(int promptId, Keys key, Keys modifiers, string prompt, bool withRetry)
        {
            ThrowIfDisposed();

            if (_promptIdToBinding.TryGetValue(promptId, out var currentBinding)
                && currentBinding.Key == key && currentBinding.Modifiers == modifiers
                && _promptIdToHotkeyId.TryGetValue(promptId, out var currentHotkeyId))
            {
                _idToPrompt[currentHotkeyId] = prompt;
                return true;
            }

            var id = _nextId++;
            var fs = ToFsModifiers(modifiers) | FsModifiers.NoRepeat;

            if (!RegisterHotKeyWithRetry(id, fs, key, withRetry, promptId, prompt))
            {
                return false;
            }

            if (_promptIdToHotkeyId.TryGetValue(promptId, out var previousHotkeyId))
            {
                UnregisterHotKey(Handle, previousHotkeyId);
                _idToPrompt.Remove(previousHotkeyId);
            }

            _idToPrompt[id] = prompt;
            _promptIdToHotkeyId[promptId] = id;
            _promptIdToBinding[promptId] = (key, modifiers);
            return true;
        }

        // On startup, a previous instance of the app may have just been killed (see
        // SquirrelHooks.OnAppRun) while still holding these exact hotkeys. Windows does not
        // release a terminated process's hotkey registrations synchronously with process exit,
        // so the first RegisterHotKey call can transiently fail with ERROR_HOTKEY_ALREADY_REGISTERED
        // even though the owning process is already gone. A few short retries clear this up
        // reliably without meaningfully slowing down the case where the key really is taken.
        // Retries are skipped (withRetry: false) on the Refresh path: Refresh already tore down
        // every registration on this window, so there's no stale-instance race to wait out, and
        // Refresh runs on the UI thread inside WndProc where Thread.Sleep would stall the message pump.
        private bool RegisterHotKeyWithRetry(int id, FsModifiers fs, Keys key, bool withRetry, int promptId, string prompt)
        {
            var maxAttempts = withRetry ? 5 : 1;
            const int delayMs = 50;
            var lastWin32Error = 0;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (RegisterHotKey(Handle, id, (uint)fs, (uint)key))
                {
                    return true;
                }

                lastWin32Error = Marshal.GetLastWin32Error();

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(delayMs);
                }
            }

            Log.Warning(
                "Failed to register hotkey for prompt {Prompt} (promptId: {PromptId}, key: {Key}, modifiers: {Modifiers}, win32Error: {Win32Error})",
                prompt, promptId, key, fs, lastWin32Error);

            return false;
        }

        // Exposed for integration tests to assert on registration state without reaching into
        // private fields via reflection.
        internal int? TryGetHotkeyId(int promptId)
        {
            return _promptIdToHotkeyId.TryGetValue(promptId, out var id) ? id : null;
        }

        // Internal (rather than private) so tests can exercise this exact code path directly -
        // the OS message pump swallows exceptions escaping a real WndProc, which makes it
        // impossible to observe clipboard-failure handling through the full Win32 round trip.
        // Returns whether id resolved to a registered prompt.
        internal bool TriggerHotkey(int id)
        {
            ThrowIfDisposed();

            if (!_idToPrompt.TryGetValue(id, out var prompt))
            {
                return false;
            }

            string userInput;
            try
            {
                userInput = ClipboardTextProvider();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read clipboard text for hotkey (prompt: {Prompt})", prompt);
                return true;
            }

            _processInputEventService.TriggerProcessStart(new ProcessStartDto(userInput, prompt));
            return true;
        }

        private static FsModifiers ToFsModifiers(Keys modifiers)
        {
            var fs = (FsModifiers)0;
            if ((modifiers & Keys.Control) == Keys.Control) fs |= FsModifiers.Control;
            if ((modifiers & Keys.Alt) == Keys.Alt) fs |= FsModifiers.Alt;
            if ((modifiers & Keys.Shift) == Keys.Shift) fs |= FsModifiers.Shift;
            return fs;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                if (TriggerHotkey(m.WParam.ToInt32()))
                {
                    return;
                }
            }
            else if (m.Msg == WM_INPUTLANGCHANGE)
            {
                // RegisterHotKey's vk parameter is a virtual-key code, not a scan code, and which
                // physical key produces a given virtual-key is resolved using the keyboard layout
                // active at registration time. When the user switches layouts, previously
                // registered hotkeys keep firing on the old physical key position unless they
                // are re-registered so Windows recomputes the mapping under the new layout.
                Refresh();
            }
            base.WndProc(ref m);
        }

        public void Refresh()
        {
            ThrowIfDisposed();

            UnregisterAll();
            RegisterAll(withRetry: false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            UnregisterAll();
            DestroyHandle();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HotKeyListener));
            }
        }
    }
}