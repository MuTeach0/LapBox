using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LapBox.Infrastructure.Data.Persistence;

/// <summary>
/// Design-time factory ensures EF tools can create the DbContext and read the connection string from .env
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Read connection string directly from .env file
        var envPath = FindEnvFile();
        
        // Try environment variable first (for CI/CD where .env might not be accessible)
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        
        // If not found via env var, try reading from .env file
        if (string.IsNullOrWhiteSpace(conn) && !string.IsNullOrEmpty(envPath))
        {
            conn = GetConnectionStringFromEnv();
        }
        
        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException(
                $"ConnectionStrings__DefaultConnection not found. Searched .env at: {envPath ?? "not found"}. " +
                $"Set ConnectionStrings__DefaultConnection environment variable or add it to .env file.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(conn, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string? GetConnectionStringFromEnv()
    {
        var envPath = FindEnvFile();
        if (string.IsNullOrEmpty(envPath) || !File.Exists(envPath))
            return null;

        // Read all lines from .env and find ConnectionStrings__DefaultConnection
        var lines = File.ReadAllLines(envPath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;
            
            // Parse KEY=VALUE format
            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = trimmed[..equalsIndex].Trim();
                var value = trimmed[(equalsIndex + 1)..].Trim();
                
                // Remove surrounding quotes if present
                if ((value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value[1..^1];
                }
                
                if (key.Equals("ConnectionStrings__DefaultConnection", StringComparison.OrdinalIgnoreCase))
                    return value;
            }
        }
        
        return null;
    }

    private static string? FindEnvFile()
    {
        // EF tools run from bin/Debug, so search from multiple possible locations
        var possibleRoots = new[]
        {
            Environment.CurrentDirectory,
            AppDomain.CurrentDomain.BaseDirectory,
            Path.GetDirectoryName(typeof(DesignTimeDbContextFactory).Assembly.Location),
            // Go up from bin folder to project root
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", "..", "..", "..", "..", "..")),
        };

        foreach (var root in possibleRoots.Where(p => !string.IsNullOrEmpty(p)))
        {
            // Try from this root going up to find .env
            var current = root;
            while (!string.IsNullOrEmpty(current))
            {
                var candidate = Path.Combine(current, ".env");
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
                
                var parent = Directory.GetParent(current!);
                if (parent == null) break;
                current = parent.FullName;
            }
        }

        return null;
    }
}
