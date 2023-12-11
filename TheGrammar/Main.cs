
using Microsoft.Toolkit.Uwp.Notifications;

namespace TheGrammar;

public partial class Main : Form
{
    private readonly NotifyIcon trayIcon;
    private readonly ContextMenuStrip trayMenu;

    public Main()
    {
        InitializeComponent();

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("CTRL + SHIFT + Y", null, OnHotKey!);
        trayMenu.Items.Add("Exit", null, OnExit!);

        // Create a tray icon
        trayIcon = new NotifyIcon
        {
            Text = "The Grammar App",
            Icon = new Icon("logo.ico", 40, 40),
            ContextMenuStrip = trayMenu,
            Visible = true,
        };

        trayIcon.MouseDoubleClick += TrayIcon_MouseDoubleClick!;
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

    private void OnHotKey(object sender, EventArgs e)
    {
        GlobalHotKeyHandler.ModifyClipboardTextAsync();
    }

    private void SubmitButton_Click(object sender, EventArgs e)
    {
        Properties.Settings.Default.ApiKey = ApiKeyTextBox.Text;
        Properties.Settings.Default.Prompt = PrompTextBox.Text;
        Properties.Settings.Default.Save();
        MessageBox.Show("Settings saved.");
    }

    private void Main_Load(object sender, EventArgs e)
    {
        ApiKeyTextBox.Text = Properties.Settings.Default.ApiKey;
        PrompTextBox.Text = Properties.Settings.Default.Prompt;
    }

    private void Main_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            Visible = false;
            ShowInTaskbar = false;
        }
    }
}
