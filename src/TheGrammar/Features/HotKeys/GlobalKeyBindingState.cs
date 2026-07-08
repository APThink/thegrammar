using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;

namespace TheGrammar.Features.HotKeys;

public interface IGlobalKeyBindingState
{
    Dictionary<int, (Keys LeftKey, Keys RightKey, string Prompt)> KeyBindings { get; }
    Task InitAsync();
}

public class GlobalKeyBindingState : IGlobalKeyBindingState
{
    public Dictionary<int, (Keys LeftKey, Keys RightKey, string Prompt)> KeyBindings { get; private set; } = new();

    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;


    public GlobalKeyBindingState(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }


    public async Task InitAsync()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var prompts = await context.Prompts.ToListAsync();

        var keyBindings = new Dictionary<int, (Keys LeftKey, Keys RightKey, string Prompt)>();

        foreach (var prompt in prompts)
        {
            keyBindings.Add(prompt.Id, (prompt.LeftKey, prompt.RightKey, prompt.Promt));
        }

        KeyBindings = keyBindings;
    }
}
