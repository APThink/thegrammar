using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;

namespace TheGrammar.Features.HotKeys;

public interface IGlobalKeyBindingState
{
    Dictionary<(Keys, Keys), string> KeyBindings { get; }
    Task InitAsync();
}

public class GlobalKeyBindingState : IGlobalKeyBindingState
{
    public Dictionary<(Keys, Keys), string> KeyBindings { get; private set; } = new();

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;


    public GlobalKeyBindingState(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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
