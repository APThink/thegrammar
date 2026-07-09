using TheGrammar.Features.LocalAI;
using TheGrammar.Features.Settings;

namespace TheGrammar.Tests;

public class FoundryLocalCatalogServiceTests
{
  [Fact]
  public async Task GetChatClientForCurrentModelAsync_Throws_WhenNoAliasConfigured()
  {
    var service = new FoundryLocalCatalogService(
      new TestOptionsMonitor<SettingsOption>(new SettingsOption { LocalModelAlias = null }));

    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetChatClientForCurrentModelAsync());

    Assert.Contains("Settings", ex.Message);
  }
}
