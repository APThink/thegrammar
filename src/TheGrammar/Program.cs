using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.Prompts;
using TheGrammar.Features.OpenAI;
using TheGrammar.Database;
using MudBlazor.Services;
using Squirrel;
using Serilog;
using TheGrammar.Features.Settings;

namespace TheGrammar;
internal static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            SquirrelAwareApp.HandleEvents(onInitialInstall: SquirrelHooks.OnAppInstall, 
                onAppUninstall: SquirrelHooks.OnAppUninstall, 
                onEveryRun: SquirrelHooks.OnAppRun);

            InitLogger();
            RunApplication();
        }
        catch (Exception ex)
        {
            var message = $"Failed to start application: {ex.Message}";
            Log.Error(message);
            MessageBox.Show(message);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void RunApplication()
    {
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder();
       
        builder.Configuration
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        builder.Environment.ApplicationName = "The Grammar";

        builder.Services.AddWindowsFormsBlazorWebView();
        builder.Services.AddMudServices();

        builder.Services.AddHotKeys();
        builder.Services.AddOpenAI(builder.Configuration);
        builder.Services.AddDatabase(builder.Configuration);
        builder.Services.AddSettings(builder.Configuration);

        builder.Services.AddScoped<PromptRepository>();

        var app = builder.Build();

        app.SetAutoStart();

        app.SeedDbAsync().Wait();

        InitBindingsState(app).Wait();

        app.Start();

        Application.Run(new MainForm(app));
        app.StopAsync().Wait();
    }

    private static async Task InitBindingsState(IHost app)
    {
        var keyBindingState = app.Services.GetRequiredService<IGlobalKeyBindingState>();
        await keyBindingState.InitAsync();
    }

    private static void InitLogger()
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs/TheGrammar.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Month)
            .CreateLogger();
    }
}