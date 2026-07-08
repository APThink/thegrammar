using System.Runtime.InteropServices;
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
            Win = 0x0008,
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
        // OS hotkey id -> prompt text, used to resolve WM_HOTKEY messages
        private readonly Dictionary<int, string> _idToPrompt = new();
        private int _nextId = 1;

        public HotKeyListener(
            IGlobalKeyBindingState keyBindingState,
            IProcessInputEventService processInputEventService)
        {
            _keyBindingState = keyBindingState;
            _processInputEventService = processInputEventService;

            var cp = new CreateParams { Caption = "TheGrammarHotKeyWindow" };
            CreateHandle(cp);
        }

        public void RegisterAll()
        {
            foreach (var (promptId, binding) in _keyBindingState.KeyBindings)
            {
                Register(promptId, binding.RightKey, binding.LeftKey, binding.Prompt);
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
        }

        // Registers (or replaces) the hotkey for a single prompt without touching any other
        // currently-registered hotkey.
        public void Register(int promptId, Keys key, Keys modifiers, string prompt)
        {
            if (_promptIdToHotkeyId.TryGetValue(promptId, out var previousHotkeyId))
            {
                UnregisterHotKey(Handle, previousHotkeyId);
                _idToPrompt.Remove(previousHotkeyId);
                _promptIdToHotkeyId.Remove(promptId);
            }

            var id = _nextId++;
            var fs = ToFsModifiers(modifiers) | FsModifiers.NoRepeat;

            if (RegisterHotKeyWithRetry(id, fs, key))
            {
                _idToPrompt[id] = prompt;
                _promptIdToHotkeyId[promptId] = id;
            }
        }

        // On startup, a previous instance of the app may have just been killed (see
        // SquirrelHooks.OnAppRun) while still holding these exact hotkeys. Windows does not
        // release a terminated process's hotkey registrations synchronously with process exit,
        // so the first RegisterHotKey call can transiently fail with ERROR_HOTKEY_ALREADY_REGISTERED
        // even though the owning process is already gone. A few short retries clear this up
        // reliably without meaningfully slowing down the case where the key really is taken.
        private bool RegisterHotKeyWithRetry(int id, FsModifiers fs, Keys key)
        {
            const int maxAttempts = 5;
            const int delayMs = 50;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (RegisterHotKey(Handle, id, (uint)fs, (uint)key))
                {
                    return true;
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(delayMs);
                }
            }

            return false;
        }

        // Exposed for integration tests to assert on registration state without reaching into
        // private fields via reflection.
        internal int? TryGetHotkeyId(int promptId)
        {
            return _promptIdToHotkeyId.TryGetValue(promptId, out var id) ? id : null;
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
                var id = m.WParam.ToInt32();
                if (_idToPrompt.TryGetValue(id, out var prompt))
                {
                    var userInput = Clipboard.GetText();
                    _processInputEventService.TriggerProcessStart(
                        new ProcessStartDto(userInput, prompt));
                    return;
                }
            }
            else if (m.Msg == WM_INPUTLANGCHANGE)
            {
                // The scan code RegisterHotKey binds to is resolved using the keyboard layout
                // active at registration time. When the user switches layouts, previously
                // registered hotkeys keep firing on the old physical key position unless they
                // are re-registered so Windows recomputes the mapping under the new layout.
                Refresh();
            }
            base.WndProc(ref m);
        }

        public void Refresh()
        {
            UnregisterAll();
            RegisterAll();
        }

        public void Dispose()
        {
            UnregisterAll();
            DestroyHandle();
        }
    }
}