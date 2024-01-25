using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheGrammar.Features.Settings;

public static class DependencyInjection
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SettingsOption>(configuration.GetSection(SettingsOption.SectionName));
        return services;
    }
}
