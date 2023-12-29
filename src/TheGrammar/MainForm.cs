using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TheGrammar.Features.HotKeys.Services;

namespace TheGrammar;

public partial class MainForm : Form
{
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _trayMenu;
    private readonly HotKeyListener _globalHotKeyHandler;

    private readonly nint _globalKeyHandlerHookId;
    public MainForm(IHost app)
    {
        InitializeComponent();

        blazorWebView1.HostPage = "wwwroot\\index.html";
        blazorWebView1.Services = app.Services;
        blazorWebView1.RootComponents.Add<App>("#app");

        _globalHotKeyHandler = app.Services.GetRequiredService<HotKeyListener>();

        _globalHotKeyHandler.SetHook(_globalHotKeyHandler._proc);
        _globalKeyHandlerHookId = HotKeyListener._hookID;

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("DevTools", null, OnDevTool!);
        _trayMenu.Items.Add("Exit", null, OnExit!);

        _trayIcon = new NotifyIcon
        {
            Text = "The Grammar App",
            Icon = new Icon("logo.ico", 40, 40),
            ContextMenuStrip = _trayMenu,
            Visible = true,
        };

        _trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick!;
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
        Text = $"The Grammar App v{version?.Major}.{version?.Minor}.{version?.Build}";
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
}
