using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;
using TheGrammar.Domain;

namespace TheGrammar.Features.Settings;

public class ModelRepository
{
  private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

  public ModelRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
  {
    _dbContextFactory = dbContextFactory;
  }

  public async Task AddModelAsync(Model model)
  {
    await using var context = await _dbContextFactory.CreateDbContextAsync();
    context.Models.Add(model);
    await context.SaveChangesAsync();
  }
}