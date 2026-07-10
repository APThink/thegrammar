using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGrammar.Database;
using TheGrammar.Domain;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.HotKeys.Events;
using TheGrammar.Features.HotKeys.Services;
using TheGrammar.Features.Prompts;

namespace TheGrammar.Tests;

// These tests exercise the real Win32 RegisterHotKey/UnregisterHotKey calls and a real SQLite
// database, since the bugs being guarded against here were in that exact integration, not in any
// single isolated unit. Hotkeys are simulated by posting WM_HOTKEY / WM_INPUTLANGCHANGE directly
// to the listener's window rather than simulating real keystrokes: real global hotkeys are a
// system-wide, session-shared resource, so driving them via SendInput would be flaky in CI
// (focus/session quirks, and collisions with whatever else is registered on the machine).
// Uncommon modifier+key combinations (Ctrl+Alt+Shift+F13/F14/F15) are used to minimize the
// (already small) chance of colliding with a real shortcut on the test machine.
[Collection("HotKeyListener (not parallel - global OS hotkey state)")]
public class HotKeyListenerIntegrationTests : IDisposable
{
  private const int WM_HOTKEY = 0x0312;
  private const int WM_INPUTLANGCHANGE = 0x0051;

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool PostMessage(nint hWnd, int msg, nint wParam, nint lParam);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool UnregisterHotKey(nint hWnd, int id);

  private readonly string _dbPath;
  private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
  private readonly IGlobalKeyBindingState _keyBindingState;
  private readonly PromptRepository _promptRepository;

  public HotKeyListenerIntegrationTests()
  {
    _dbPath = Path.Combine(Path.GetTempPath(), $"thegrammar-tests-{Guid.NewGuid():N}.db");

    var services = new ServiceCollection();
    services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
    var provider = services.BuildServiceProvider();

    _dbContextFactory = provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using (var context = _dbContextFactory.CreateDbContext())
    {
      context.Database.EnsureCreated();
    }

    _keyBindingState = new GlobalKeyBindingState(_dbContextFactory);
    _promptRepository = new PromptRepository(_dbContextFactory);
  }

  public void Dispose()
  {
    // Microsoft.Data.Sqlite pools native connections behind the scenes; without clearing the
    // pool first, the temp db file can still be locked here even though every DbContext using
    // it has already been disposed.
    SqliteConnection.ClearAllPools();

    if (File.Exists(_dbPath))
    {
      File.Delete(_dbPath);
    }
  }

  private static T RunOnSta<T>(Func<T> action)
  {
    T result = default!;
    Exception? failure = null;

    var thread = new Thread(() =>
    {
      try
      {
        result = action();
      }
      catch (Exception ex)
      {
        failure = ex;
      }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure is not null)
    {
      throw new Exception("STA thread failed", failure);
    }

    return result;
  }

  private static void PostHotkey(nint handle, int hotkeyId)
  {
    PostMessage(handle, WM_HOTKEY, hotkeyId, 0);
    Application.DoEvents();
  }

  private static void PostLayoutChange(nint handle)
  {
    PostMessage(handle, WM_INPUTLANGCHANGE, 0, 0);
    Application.DoEvents();
  }

  private static Prompt NewPrompt(Keys rightKey, string text) => new()
  {
    LeftKey = Keys.Control | Keys.Alt | Keys.Shift,
    RightKey = rightKey,
    Promt = text
  };

  [Fact]
  public void AddingNewHotkey_RegistersImmediately_AndDoesNotBreakExistingHotkeys()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var promptA = NewPrompt(Keys.F13, "prompt-a");
      var promptB = NewPrompt(Keys.F14, "prompt-b");
      _promptRepository.AddPromptAsync(promptA).GetAwaiter().GetResult();
      _promptRepository.AddPromptAsync(promptB).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var idA = listener.TryGetHotkeyId(promptA.Id);
      var idB = listener.TryGetHotkeyId(promptB.Id);
      Assert.NotNull(idA);
      Assert.NotNull(idB);

      // sanity check: both hotkeys registered at startup work before any add happens
      PostHotkey(listener.Handle, idA.Value);
      PostHotkey(listener.Handle, idB.Value);
      Assert.Equal(2, received.Count);
      Assert.Equal("prompt-a", received[0].Prompt);
      Assert.Equal("prompt-b", received[1].Prompt);

      // act: add a third prompt the same way AddPromptDialog does - a single targeted
      // Register call, not a full Refresh()
      var promptC = NewPrompt(Keys.F15, "prompt-c");
      _promptRepository.AddPromptAsync(promptC).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();
      listener.Register(promptC.Id, promptC.RightKey, promptC.LeftKey, promptC.Promt);

      // bug #1: the new hotkey must fire immediately, with no further trigger
      var idC = listener.TryGetHotkeyId(promptC.Id);
      Assert.NotNull(idC);
      PostHotkey(listener.Handle, idC.Value);
      Assert.Equal(3, received.Count);
      Assert.Equal("prompt-c", received[2].Prompt);

      // bug #2: the previously-registered hotkeys must still work, unchanged
      PostHotkey(listener.Handle, idA.Value);
      PostHotkey(listener.Handle, idB.Value);
      Assert.Equal(5, received.Count);
      Assert.Equal("prompt-a", received[3].Prompt);
      Assert.Equal("prompt-b", received[4].Prompt);

      return true;
    });
  }

  [Fact]
  public void EditingHotkey_UpdatesRegistration_WithoutRestart()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var prompt = NewPrompt(Keys.F13, "before-edit");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var originalId = listener.TryGetHotkeyId(prompt.Id);
      Assert.NotNull(originalId);
      PostHotkey(listener.Handle, originalId.Value);
      Assert.Single(received);
      Assert.Equal("before-edit", received[0].Prompt);

      // act: edit the prompt's key binding and text, the way PromptView.ChangePromptAsync does
      prompt.RightKey = Keys.F16;
      prompt.Promt = "after-edit";
      _promptRepository.UpdatePromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();
      listener.Register(prompt.Id, prompt.RightKey, prompt.LeftKey, prompt.Promt);

      var updatedId = listener.TryGetHotkeyId(prompt.Id);
      Assert.NotNull(updatedId);
      Assert.NotEqual(originalId, updatedId);

      // the old combo must no longer trigger anything
      PostHotkey(listener.Handle, originalId.Value);
      Assert.Single(received);

      // the new combo must trigger immediately, with the updated prompt text
      PostHotkey(listener.Handle, updatedId.Value);
      Assert.Equal(2, received.Count);
      Assert.Equal("after-edit", received[1].Prompt);

      return true;
    });
  }

  [Fact]
  public void KeyboardLayoutChange_ReRegistersAllHotkeys()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var promptA = NewPrompt(Keys.F13, "prompt-a");
      var promptB = NewPrompt(Keys.F14, "prompt-b");
      _promptRepository.AddPromptAsync(promptA).GetAwaiter().GetResult();
      _promptRepository.AddPromptAsync(promptB).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var originalIdA = listener.TryGetHotkeyId(promptA.Id);
      var originalIdB = listener.TryGetHotkeyId(promptB.Id);
      Assert.NotNull(originalIdA);
      Assert.NotNull(originalIdB);

      // act: simulate an OS keyboard layout switch
      PostLayoutChange(listener.Handle);

      // the listener must have re-registered every hotkey (new OS ids), so it recomputes
      // the scan-code mapping under the newly active layout
      var newIdA = listener.TryGetHotkeyId(promptA.Id);
      var newIdB = listener.TryGetHotkeyId(promptB.Id);
      Assert.NotNull(newIdA);
      Assert.NotNull(newIdB);
      Assert.NotEqual(originalIdA, newIdA);
      Assert.NotEqual(originalIdB, newIdB);

      // old ids are gone, new ids resolve to the right prompts
      PostHotkey(listener.Handle, originalIdA.Value);
      PostHotkey(listener.Handle, originalIdB.Value);
      Assert.Empty(received);

      PostHotkey(listener.Handle, newIdA.Value);
      PostHotkey(listener.Handle, newIdB.Value);
      Assert.Equal(2, received.Count);
      Assert.Equal("prompt-a", received[0].Prompt);
      Assert.Equal("prompt-b", received[1].Prompt);

      return true;
    });
  }

  [Fact]
  public void EditingHotkey_WhenNewCombinationIsAlreadyTaken_KeepsOldHotkeyWorking()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var prompt = NewPrompt(Keys.F13, "before-edit");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var originalId = listener.TryGetHotkeyId(prompt.Id);
      Assert.NotNull(originalId);

      // Occupy the combo we're about to move the prompt to from a separate window, so the
      // listener's re-registration attempt genuinely fails (mirrors a real key collision)
      // rather than being simulated.
      const int blockerId = 999;
      const uint modAltControlShiftNoRepeat = 0x0001 | 0x0002 | 0x0004 | 0x4000;
      var blocker = new NativeWindow();
      blocker.CreateHandle(new CreateParams { Caption = "hotkey-blocker" });

      try
      {
        Assert.True(RegisterHotKey(blocker.Handle, blockerId, modAltControlShiftNoRepeat, (uint)Keys.F18));

        prompt.RightKey = Keys.F18;
        prompt.Promt = "after-edit";
        _promptRepository.UpdatePromptAsync(prompt).GetAwaiter().GetResult();
        _keyBindingState.InitAsync().GetAwaiter().GetResult();

        // bug #3: a failed re-registration must not tear down the hotkey that was already working
        var registered = listener.Register(prompt.Id, prompt.RightKey, prompt.LeftKey, prompt.Promt);
        Assert.False(registered);

        var idAfterFailedRegister = listener.TryGetHotkeyId(prompt.Id);
        Assert.Equal(originalId, idAfterFailedRegister);

        PostHotkey(listener.Handle, originalId.Value);
        Assert.Single(received);
        Assert.Equal("before-edit", received[0].Prompt);
      }
      finally
      {
        UnregisterHotKey(blocker.Handle, blockerId);
        blocker.DestroyHandle();
      }

      return true;
    });
  }

  [Fact]
  public void RegisterAll_BeforeKeyBindingStateInitialized_ThrowsInsteadOfSilentlyRegisteringNothing()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      using var listener = new HotKeyListener(_keyBindingState, events);

      // _keyBindingState.InitAsync() was never called - KeyBindings would silently be empty
      Assert.Throws<InvalidOperationException>(() => listener.RegisterAll());

      return true;
    });
  }

  [Fact]
  public void ClipboardReadFailure_DoesNotCrashListener()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var prompt = NewPrompt(Keys.F13, "prompt-a");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events)
      {
        ClipboardTextProvider = () => throw new COMException("CLIPBRD_E_CANT_OPEN", unchecked((int)0x800401D0))
      };
      listener.RegisterAll();

      var id = listener.TryGetHotkeyId(prompt.Id);
      Assert.NotNull(id);

      // Exercise the exact WndProc code path directly: the real OS message pump swallows
      // exceptions that escape a window procedure in a way that can't be reliably observed
      // from a test (see TriggerHotkey's doc comment), so we can't prove this through
      // PostMessage/DoEvents alone.
      var thrown = Record.Exception(() => listener.TriggerHotkey(id!.Value));

      Assert.Null(thrown);
      Assert.Empty(received);

      return true;
    });
  }

  [Fact]
  public void EditingOnePrompt_WithUnchangedKeyCombination_DoesNotBreakOtherPrompts()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var promptA = NewPrompt(Keys.F13, "prompt-a");
      var promptB = NewPrompt(Keys.F14, "prompt-b");
      _promptRepository.AddPromptAsync(promptA).GetAwaiter().GetResult();
      _promptRepository.AddPromptAsync(promptB).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var idA = listener.TryGetHotkeyId(promptA.Id);
      var idB = listener.TryGetHotkeyId(promptB.Id);

      // act: save promptA unchanged, exactly like PromptView.ChangePromptAsync
      _promptRepository.UpdatePromptAsync(promptA).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();
      var registered = listener.Register(promptA.Id, promptA.RightKey, promptA.LeftKey, promptA.Promt);
      Assert.True(registered);

      // promptB must still fire
      PostHotkey(listener.Handle, idB!.Value);
      Assert.Single(received);
      Assert.Equal("prompt-b", received[0].Prompt);

      // promptA must still fire too
      PostHotkey(listener.Handle, idA!.Value);
      Assert.Equal(2, received.Count);
      Assert.Equal("prompt-a", received[1].Prompt);

      return true;
    });
  }

  [Fact]
  public void EditingPromptText_WithUnchangedKeyCombination_UpdatesTextWithoutReRegistering()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var received = new List<ProcessStartDto>();
      events.ProcessStartEvents.Subscribe(received.Add);

      var prompt = NewPrompt(Keys.F13, "before-edit");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      var originalId = listener.TryGetHotkeyId(prompt.Id);
      Assert.NotNull(originalId);

      // act: edit only the prompt text, keeping the same key combination - the way PromptView
      // does when a user just tweaks the wording without touching the shortcut
      prompt.Promt = "after-edit";
      _promptRepository.UpdatePromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      // bug #4: re-registering the same combination must not fail (the old id still owns that
      // combo, so a naive re-register-under-a-new-id would lose to it) and must pick up the new text
      var registered = listener.Register(prompt.Id, prompt.RightKey, prompt.LeftKey, prompt.Promt);
      Assert.True(registered);

      var idAfterEdit = listener.TryGetHotkeyId(prompt.Id);
      Assert.Equal(originalId, idAfterEdit);

      PostHotkey(listener.Handle, originalId.Value);
      Assert.Single(received);
      Assert.Equal("after-edit", received[0].Prompt);

      return true;
    });
  }

  [Fact]
  public void Refresh_WithACollidingCombination_DoesNotBlockOnRetries()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var prompt = NewPrompt(Keys.F13, "prompt-a");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      using var listener = new HotKeyListener(_keyBindingState, events);
      listener.RegisterAll();

      // Occupy the prompt's combo from a separate window so Refresh's re-registration of it
      // genuinely fails, the way a real collision after a layout switch would.
      const int blockerId = 998;
      const uint modAltControlShiftNoRepeat = 0x0001 | 0x0002 | 0x0004 | 0x4000;
      var blocker = new NativeWindow();
      blocker.CreateHandle(new CreateParams { Caption = "hotkey-blocker" });

      try
      {
        // Refresh unregisters everything first, so this only succeeds once the listener has
        // released its own registration for prompt-a.
        listener.UnregisterAll();
        Assert.True(RegisterHotKey(blocker.Handle, blockerId, modAltControlShiftNoRepeat, (uint)Keys.F13));

        var stopwatch = Stopwatch.StartNew();
        listener.Refresh();
        stopwatch.Stop();

        // bug #5: Refresh runs on the UI thread inside WndProc. A single failed attempt is
        // near-instant; the retry path used at startup sleeps 4 x 50ms = 200ms between attempts,
        // so a comfortably lower bound proves the retries were skipped rather than merely fast.
        Assert.True(stopwatch.ElapsedMilliseconds < 150,
          $"Refresh took {stopwatch.ElapsedMilliseconds}ms, expected it to skip retry sleeps entirely");
      }
      finally
      {
        UnregisterHotKey(blocker.Handle, blockerId);
        blocker.DestroyHandle();
      }

      return true;
    });
  }

  [Fact]
  public void Dispose_CalledTwice_DoesNotThrow()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var listener = new HotKeyListener(_keyBindingState, events);

      listener.Dispose();
      var thrown = Record.Exception(() => listener.Dispose());

      Assert.Null(thrown);
      return true;
    });
  }

  [Fact]
  public void MethodsCalledAfterDispose_ThrowObjectDisposedException()
  {
    RunOnSta(() =>
    {
      var events = new ProcessInputEventService();
      var prompt = NewPrompt(Keys.F13, "prompt-a");
      _promptRepository.AddPromptAsync(prompt).GetAwaiter().GetResult();
      _keyBindingState.InitAsync().GetAwaiter().GetResult();

      var listener = new HotKeyListener(_keyBindingState, events);
      listener.Dispose();

      Assert.Throws<ObjectDisposedException>(() => listener.RegisterAll());
      Assert.Throws<ObjectDisposedException>(
        () => listener.Register(prompt.Id, prompt.RightKey, prompt.LeftKey, prompt.Promt));
      Assert.Throws<ObjectDisposedException>(() => listener.Refresh());
      Assert.Throws<ObjectDisposedException>(() => listener.TriggerHotkey(1));

      return true;
    });
  }
}