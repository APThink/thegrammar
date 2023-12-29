using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;
using TheGrammar.Domain;

namespace TheGrammar.Features.Prompts;

public class PromptRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public PromptRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Prompt>> GetPromptsAsync()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return await context.Prompts.ToListAsync();
    }

    public async Task AddPromptAsync(Prompt prompt)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Prompts.Add(prompt);
        await context.SaveChangesAsync();
    }

    public async Task UpdatePromptAsync(Prompt prompt)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Prompts.Update(prompt);
        await context.SaveChangesAsync();
    }
}