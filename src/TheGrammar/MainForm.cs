using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TheGrammar.Features.HotKeys.Services;
using TheGrammar.Features.HotKeys.Events;

namespace TheGrammar;

public partial class MainForm : Form
{
  private readonly NotifyIcon _trayIcon;
  private readonly ContextMenuStrip _trayMenu;
  private readonly HotKeyListener _globalHotKeyHandler;
  private Icon[] animationFrames;
  private System.Windows.Forms.Timer animationTimer;
  private int currentFrameIndex;
  private IDisposable? processStartSubscription;
  private IDisposable? processStopSubscription;
  private IDisposable? processCanceledSubscription;
  private readonly IProcessInputEventService _processEventService;

  private readonly nint _globalKeyHandlerHookId;

  public MainForm(IHost app)
  {
    InitializeComponent();

    var hostPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot\\index.html");

    blazorWebView1.HostPage = hostPage;
    blazorWebView1.Services = app.Services;
    blazorWebView1.RootComponents.Add<App>("#app");

    _globalHotKeyHandler = app.Services.GetRequiredService<HotKeyListener>();

    _globalHotKeyHandler.SetHook(_globalHotKeyHandler._proc);
    _globalKeyHandlerHookId = HotKeyListener._hookID;

    _trayMenu = new ContextMenuStrip();
    _trayMenu.Items.Add("DevTools", null, OnDevTool!);
    _trayMenu.Items.Add("Exit", null, OnExit!);
    _trayMenu.Items.Add("Cancel Request", null, OnCancel!);

    _trayIcon = new NotifyIcon
    {
      Text = "The Grammar App",
      Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico"), 40, 40),
      ContextMenuStrip = _trayMenu,
      Visible = true,
    };

    // Load your animation frames
    animationFrames = new Icon[]
    {
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh0.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh1.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh2.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh3.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh4.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh5.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh6.ico"), 128, 128),
      new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/refresh7.ico"), 128, 128),
    };
    currentFrameIndex = 0;

    animationTimer = new System.Windows.Forms.Timer
    {
      Interval = 150
    };

    animationTimer.Tick += AnimationTimer_Tick!;

    _trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick!;

    _processEventService = app.Services.GetRequiredService<IProcessInputEventService>();
    processStartSubscription = _processEventService.ProcessStartEvents.Subscribe(_ => { StartAnimation(); });
    processStopSubscription = _processEventService.ProcessFinishEvents.Subscribe(_ => { StopAnimation(); });
    processCanceledSubscription = _processEventService.ProcessCancellationEvents.Subscribe(_ => { StopAnimation(); });
  }

  private void OnDevTool(object sender, EventArgs e)
  {
    if (blazorWebView1.WebView.CoreWebView2 is null)
    {
      MessageBox.Show("DevTools is not ready yet.");
      return;
    }

    blazorWebView1.WebView.CoreWebView2.OpenDevToolsWindow();
  }

  private void OnCancel(object sender, EventArgs e)
  {
    _processEventService.TriggerProcessCancellation();
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);
    Visible = false;
    ShowInTaskbar = false;
  }

  private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
  {
    Show();
    WindowState = FormWindowState.Normal;
    ShowInTaskbar = true;
  }

  private void OnExit(object sender, EventArgs e)
  {
    Application.Exit();
  }

  private void Main_Load(object sender, EventArgs e)
  {
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    Text = $@"The Grammar App v{version?.Major}.{version?.Minor}.{version?.Build}";
  }

  private void Main_Resize(object sender, EventArgs e)
  {
    if (this.WindowState == FormWindowState.Minimized)
    {
      Visible = false;
      ShowInTaskbar = false;
    }
  }

  private void Main_FormClosing(object sender, FormClosingEventArgs e)
  {
    if (e.CloseReason == CloseReason.UserClosing)
    {
      e.Cancel = true;
      Visible = false;
      ShowInTaskbar = false;
    }

    HotKeyListener.UnhookWindowsHookEx(_globalKeyHandlerHookId);
  }


  private void StartAnimation()
  {
    animationTimer.Start();
  }

  private void StopAnimation()
  {
    animationTimer.Stop();
    _trayIcon.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico"), 128, 128);
  }

  private void AnimationTimer_Tick(object sender, EventArgs e)
  {
    _trayIcon.Icon = animationFrames[currentFrameIndex];
    currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
  }

  private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
  {
    processStartSubscription!.Dispose(); 
    processStopSubscription!.Dispose(); 
    processCanceledSubscription!.Dispose();
  }
}