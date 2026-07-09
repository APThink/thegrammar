using TheGrammar.Features.PrompProcessor;
using TheGrammar.Features.Settings;

namespace TheGrammar.Tests;

public class AiServiceResolverTests
{
  private sealed class StubAiService : IAiService
  {
    public Task<AiResult> ProcessAsync(string prompt, string input, CancellationToken ct = default)
      => throw new NotSupportedException("Not used by resolver tests");
  }

  [Fact]
  public void GetCurrentService_ReturnsOpenAiService_WhenProviderIsOpenAi()
  {
    var openAi = new StubAiService();
    var local = new StubAiService();
    var resolver = new AiServiceResolver(
      openAi,
      local,
      new TestOptionsMonitor<SettingsOption>(new SettingsOption { AiProvider = AiProvider.OpenAi }));

    var result = resolver.GetCurrentService();

    Assert.Same(openAi, result);
  }

  [Fact]
  public void GetCurrentService_ReturnsLocalService_WhenProviderIsLocal()
  {
    var openAi = new StubAiService();
    var local = new StubAiService();
    var resolver = new AiServiceResolver(
      openAi,
      local,
      new TestOptionsMonitor<SettingsOption>(new SettingsOption { AiProvider = AiProvider.Local }));

    var result = resolver.GetCurrentService();

    Assert.Same(local, result);
  }
}
