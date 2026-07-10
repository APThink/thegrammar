using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGrammar.Database;
using TheGrammar.Domain;
using TheGrammar.Features.Prompts;

namespace TheGrammar.Tests;

public class PromptRepositoryTests : IDisposable
{
  private readonly string _dbPath;
  private readonly PromptRepository _promptRepository;

  public PromptRepositoryTests()
  {
    _dbPath = Path.Combine(Path.GetTempPath(), $"thegrammar-tests-{Guid.NewGuid():N}.db");

    var services = new ServiceCollection();
    services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
    var provider = services.BuildServiceProvider();

    var dbContextFactory = provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using (var context = dbContextFactory.CreateDbContext())
    {
      context.Database.EnsureCreated();
    }

    _promptRepository = new PromptRepository(dbContextFactory);
  }

  public void Dispose()
  {
    SqliteConnection.ClearAllPools();

    if (File.Exists(_dbPath))
    {
      File.Delete(_dbPath);
    }
  }

  private static Prompt NewPrompt(Keys rightKey, string text, Keys? leftKey = null) => new()
  {
    LeftKey = leftKey ?? (Keys.Control | Keys.Shift),
    RightKey = rightKey,
    Promt = text
  };

  [Fact]
  public async Task AddPromptAsync_DuplicateHotkey_ThrowsDuplicateHotkeyException()
  {
    await _promptRepository.AddPromptAsync(NewPrompt(Keys.A, "prompt-a"));

    await Assert.ThrowsAsync<DuplicateHotkeyException>(() =>
      _promptRepository.AddPromptAsync(NewPrompt(Keys.A, "prompt-b")));
  }

  [Fact]
  public async Task UpdatePromptAsync_ToHotkeyUsedByAnotherPrompt_ThrowsDuplicateHotkeyException()
  {
    var promptA = NewPrompt(Keys.A, "prompt-a");
    var promptB = NewPrompt(Keys.B, "prompt-b");
    await _promptRepository.AddPromptAsync(promptA);
    await _promptRepository.AddPromptAsync(promptB);

    promptB.RightKey = Keys.A;

    await Assert.ThrowsAsync<DuplicateHotkeyException>(() => _promptRepository.UpdatePromptAsync(promptB));
  }

  [Fact]
  public async Task UpdatePromptAsync_WithUnchangedHotkey_DoesNotThrow()
  {
    var prompt = NewPrompt(Keys.A, "before-edit");
    await _promptRepository.AddPromptAsync(prompt);

    prompt.Promt = "after-edit";

    await _promptRepository.UpdatePromptAsync(prompt);

    var reloaded = await _promptRepository.GetPromptsAsync();
    Assert.Equal("after-edit", Assert.Single(reloaded).Promt);
  }

  [Fact]
  public async Task GetUsedRightKeysAsync_ExcludesGivenPromptId_ReturnsOtherPromptsKeys()
  {
    var promptA = NewPrompt(Keys.A, "prompt-a");
    var promptB = NewPrompt(Keys.B, "prompt-b");
    await _promptRepository.AddPromptAsync(promptA);
    await _promptRepository.AddPromptAsync(promptB);

    var usedKeys =
      await _promptRepository.GetUsedRightKeysAsync(Keys.Control | Keys.Shift, excludingPromptId: promptB.Id);

    Assert.Equal([Keys.A], usedKeys);
  }
}