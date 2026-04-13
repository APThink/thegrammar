using System.Runtime.InteropServices;
using TheGrammar.Features.HotKeys.Events;

namespace TheGrammar.Features.HotKeys.Services
{
    public class HotKeyListener : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

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
            foreach (var binding in _keyBindingState.KeyBindings)
            {
                var (key, modifiers) = binding.Key;
                var prompt = binding.Value;
                var id = _nextId++;

                var fs = ToFsModifiers(modifiers) | FsModifiers.NoRepeat;

                if (RegisterHotKey(Handle, id, (uint)fs, (uint)key))
                {
                    _idToPrompt[id] = prompt;
                }
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _idToPrompt.Keys)
            {
                UnregisterHotKey(Handle, id);
            }
            _idToPrompt.Clear();
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