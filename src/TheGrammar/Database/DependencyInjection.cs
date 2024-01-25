using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheGrammar.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var originalConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (originalConnectionString == null)
        {
            throw new Exception("DefaultConnection not found in appsettings.json");
        }

        var relativePath = originalConnectionString.Replace("Data Source=", "").Trim(';');
        var absoluteDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        
        var newConnectionString = $"DataSource={absoluteDbPath}";

        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseSqlite(newConnectionString);
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(newConnectionString);
        });

        return services;
    }
}
