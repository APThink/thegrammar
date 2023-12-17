using Microsoft.EntityFrameworkCore;
using TheGrammar.Data;

namespace TheGrammar.Services;

public class PromptService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PromptService(IDbContextFactory<AppDbContext> dbContextFactory)
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