using LapBox.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;

namespace LapBox.Infrastructure.Data.Persistence;

/// <summary>
/// Design-time factory used by EF Core CLI tools (migrations, scaffolding).
/// This bypasses the full DI container — only configures what EF Core needs.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    static string FindEnvFile()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            var candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        // fallback to default behaviour (current directory)
        return ".env";
    }
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.Load(FindEnvFile());

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
