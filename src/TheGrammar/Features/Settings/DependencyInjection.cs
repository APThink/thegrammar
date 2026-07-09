using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheGrammar.Features.HotKeys;
using TheGrammar.Features.LocalAI;
using TheGrammar.Features.OpenAI;
using TheGrammar.Features.PrompProcessor;

namespace TheGrammar.Features.Settings;

public static class DependencyInjection
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SettingsOption>(configuration.GetSection(SettingsOption.SectionName));
        
        services.AddSingleton<OpenAiServiceAdapter>();
        services.AddSingleton<FoundryLocalCatalogService>();
        services.AddSingleton<FoundryLocalAiService>();
        services.AddSingleton<AiServiceResolver>();
        
        return services;
    }
}
