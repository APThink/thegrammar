using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI_API;

namespace TheGrammar.Features.OpenAI;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ChatVersionState>();
        services.AddSingleton<OpenAiService>();

        var openApiSettings = configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>();

        if (openApiSettings is null)
        {
            MessageBox.Show("OpenAI API settings not found. Please add them to the appsettings.json file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton<IOpenAIAPI>(new OpenAIAPI(new APIAuthentication(openApiSettings!.ApiKey)));

        return services;
    }
}
