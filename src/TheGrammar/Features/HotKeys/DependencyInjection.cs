using Microsoft.Extensions.DependencyInjection;
using TheGrammar.Features.HotKeys.Events;
using TheGrammar.Features.HotKeys.Services;

namespace TheGrammar.Features.HotKeys;

public static class DependencyInjection
{
    public static IServiceCollection AddHotKeys(this IServiceCollection services)
    {
        services.AddSingleton<HotKeyListener>();
        services.AddSingleton<IGlobalKeyBindingState, GlobalKeyBindingState>();

        services.AddHostedService<InputProcessingManager>();

        services.AddSingleton<IProcessInputEventService, ProcessInputEventService>();

        return services;
    }
}
