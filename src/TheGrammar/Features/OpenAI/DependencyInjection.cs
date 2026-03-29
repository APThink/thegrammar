using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace TheGrammar.Features.OpenAI;

public static class DependencyInjection
{
  private const string SettingsNotFoundMessage =
    "OpenAI settings not found.";

  private const string Error = "Error";

  public static void AddOpenAi(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<ChatVersionState>();
    services.AddSingleton<OpenAiService>();

    var openApiSettings = configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>();

    if (openApiSettings is null)
    {
      MessageBox.Show(SettingsNotFoundMessage, Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
      Application.Exit();
    }

    services.Configure<OpenAiOptions>(configuration.GetRequiredSection(OpenAiOptions.SectionName));
    services.AddSingleton<OpenAIClient>(_ => new OpenAIClient(ApiKeyStore.Load() ?? "sk-not-configured"));
  }
}