using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;
using TheGrammar.Domain;

namespace TheGrammar.Features.Prompts;

public class PromptRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
  public async Task<List<Prompt>> GetPromptsAsync()
  {
    await using var context = await dbContextFactory.CreateDbContextAsync();
    return await context.Prompts.ToListAsync();
  }

  public async Task<List<Keys>> GetUsedRightKeysAsync(Keys leftKey, int? excludingPromptId = null)
  {
    await using var context = await dbContextFactory.CreateDbContextAsync();
    var query = context.Prompts.Where(p => p.LeftKey == leftKey);
    if (excludingPromptId is { } id)
    {
      query = query.Where(p => p.Id != id);
    }

    return await query.Select(p => p.RightKey).ToListAsync();
  }

  public async Task AddPromptAsync(Prompt prompt)
  {
    await using var context = await dbContextFactory.CreateDbContextAsync();
    context.Prompts.Add(prompt);
    await SaveChangesAsync(context, prompt);
  }

  public async Task UpdatePromptAsync(Prompt prompt)
  {
    await using var context = await dbContextFactory.CreateDbContextAsync();
    context.Prompts.Update(prompt);
    await SaveChangesAsync(context, prompt);
  }

  private static async Task SaveChangesAsync(ApplicationDbContext context, Prompt prompt)
  {
    try
    {
      await context.SaveChangesAsync();
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
    {
      throw new DuplicateHotkeyException(
        $"The hotkey {prompt} is already assigned to another prompt.", ex);
    }
  }
}