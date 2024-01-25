using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TheGrammar.Features.Settings;

public static class AutoStarter
{
    public static IHost SetAutoStart(this IHost app)
    {
        var settings = app.Services.GetRequiredService<IOptions<SettingsOption>>();

        var appName = Application.ProductName;

        if (settings.Value.AutoStartEnabled)
        {
            AutoStartService.SetAutoStart(appName, Application.ExecutablePath);
            return app;
        }

        AutoStartService.RemoveAutoStart(appName);
        return app;
    }
}
