using Microsoft.EntityFrameworkCore;
using TheGrammar.Data;

namespace TheGrammar.Services;

public class KeyBindingState
{
    public Dictionary<(Keys, Keys), string> KeyBindings = new();

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public KeyBindingState(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitAsync()
    {
        using var scope = _dbContextFactory.CreateDbContext().Database.BeginTransaction();
        var prompts = await _dbContextFactory.CreateDbContext().Prompts.ToListAsync();

        KeyBindings = new Dictionary<(Keys, Keys), string>();

        foreach (var prompt in prompts)
        {
            KeyBindings.Add((prompt.RightKey, prompt.LeftKey), prompt.Promt);
        }
    }

}
