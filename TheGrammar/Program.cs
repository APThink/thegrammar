using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using System.Diagnostics;
using TheGrammar.Services;
using TheGrammar.Data;
using OpenAI_API;
using Serilog;
using Squirrel;

namespace TheGrammar;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        SquirrelAwareApp.HandleEvents(onInitialInstall: OnAppInstall, onAppUninstall: OnAppUninstall, onEveryRun: OnAppRun);
        RunApplication();
    }

    private static void RunApplication()
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File("logs/thegrammar.txt", rollingInterval: RollingInterval.Month)
        .CreateLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Environment.ApplicationName = "The Grammar";

            builder.Services.AddWindowsFormsBlazorWebView();
            builder.Services.AddMudServices();

            builder.Services.AddSingleton<PushService>();
            builder.Services.AddSingleton<GlobalHotKeyService>();
            builder.Services.AddSingleton<OpenAiService>();
            builder.Services.AddSingleton<KeyBindingState>();
            builder.Services.AddScoped<PromptService>();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));

            var openApiSettings = builder.Configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>();

            if (openApiSettings is null)
            {
                MessageBox.Show("OpenAI API settings not found. Please add them to the appsettings.json file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            builder.Services.AddSingleton<IOpenAIAPI>(new OpenAIAPI(new APIAuthentication(openApiSettings!.ApiKey)));

            var app = builder.Build();

            SeedDbAsync(app).Wait();
            InitBindingsState(app).Wait();
            LoggerInit();

            app.Start();

            Application.Run(new MainForm(app));

            app.StopAsync().Wait();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to start application: {ex.Message}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task InitBindingsState(IHost app)
    {
        var keyBindingState = app.Services.GetRequiredService<KeyBindingState>();
        await keyBindingState.InitAsync();
    }

    private static async Task SeedDbAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
            await SeedData.SeedPrompts(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void LoggerInit()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/thegrammar.txt", rollingInterval: RollingInterval.Month)
            .CreateLogger();
    }

    private static void OnAppInstall(SemanticVersion version, IAppTools tools)
    {
        tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
    }

    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
    }

    private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        var processes = Process.GetProcessesByName(processName);

        foreach (var process in processes)
        {
            if (process.Id != Environment.ProcessId)
            {
                process.Kill();
            }
        }

        tools.SetProcessAppUserModelId();
        if (firstRun) MessageBox.Show("Thanks for installing The Grammar!", "Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

}