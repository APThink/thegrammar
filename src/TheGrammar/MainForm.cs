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
  private readonly Icon _defaultIcon;
  private Icon[] animationFrames;
  private System.Windows.Forms.Timer animationTimer;
  private int currentFrameIndex;
  private IDisposable? processStartSubscription;
  private IDisposable? processStopSubscription;
  private IDisposable? processCanceledSubscription;
  private IDisposable? processErrorSubscription;
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

    _globalHotKeyHandler.RegisterAll();

    _trayMenu = new ContextMenuStrip();
    _trayMenu.Items.Add("DevTools", null, OnDevTool!);
    _trayMenu.Items.Add("Cancel Request", null, OnCancel!);
    _trayMenu.Items.Add("Exit", null, OnExit!);

    _defaultIcon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.ico"), 40, 40);

    _trayIcon = new NotifyIcon
    {
      Text = "The Grammar App",
      Icon = _defaultIcon,
      ContextMenuStrip = _trayMenu,
      Visible = true,
    };

    animationFrames = TraySpinnerFrames.Generate();
    currentFrameIndex = 0;

    animationTimer = new System.Windows.Forms.Timer
    {
      Interval = 50
    };

    animationTimer.Tick += AnimationTimer_Tick!;

    _trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick!;

    _processEventService = app.Services.GetRequiredService<IProcessInputEventService>();
    processStartSubscription = _processEventService.ProcessStartEvents.Subscribe(_ => { StartAnimation(); });
    processStopSubscription = _processEventService.ProcessFinishEvents.Subscribe(_ => { StopAnimation(); });
    processCanceledSubscription = _processEventService.ProcessCancellationEvents.Subscribe(_ => { StopAnimation(); });
    processErrorSubscription = _processEventService.ProcessErrorEvents.Subscribe(dto =>
    {
      StopAnimation();
      ShowErrorBalloon(dto.Message);
    });
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

    _globalHotKeyHandler.Dispose();
  }


  private void ShowErrorBalloon(string message)
  {
    _trayIcon.BalloonTipIcon = ToolTipIcon.Error;
    _trayIcon.BalloonTipTitle = "Error";
    _trayIcon.BalloonTipText = message;
    _trayIcon.ShowBalloonTip(5000);
  }

  private void StartAnimation()
  {
    if (InvokeRequired) { Invoke(StartAnimation); return; }
    currentFrameIndex = 0;
    animationTimer.Start();
  }

  private void StopAnimation()
  {
    if (InvokeRequired) { Invoke(StopAnimation); return; }
    animationTimer.Stop();
    _trayIcon.Icon = _defaultIcon;
  }

  private void AnimationTimer_Tick(object sender, EventArgs e)
  {
    _trayIcon.Icon = animationFrames[currentFrameIndex];
    currentFrameIndex = (currentFrameIndex + 1) % animationFrames.Length;
  }

  private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
  {
    processStartSubscription?.Dispose();
    processStopSubscription?.Dispose();
    processCanceledSubscription?.Dispose();
    processErrorSubscription?.Dispose();
  }
}