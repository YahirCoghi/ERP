using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MiniERP.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        // If running from repo root, also look for API appsettings.
        var apiSettingsPath = Path.Combine(basePath, "src", "MiniERP.API", "appsettings.json");
        if (File.Exists(apiSettingsPath))
        {
            configBuilder.AddJsonFile(apiSettingsPath, optional: true);
        }

        var configuration = configBuilder.Build();
        var provider = configuration["DatabaseProvider"] ?? Environment.GetEnvironmentVariable("DatabaseProvider") ?? "SqlServer";
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var conn = configuration.GetConnectionString("SqlServer")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost,1433;Database=MiniERP;User Id=sa;Password=Your_password123;";
            optionsBuilder.UseSqlServer(conn);
        }
        else if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) || provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            var conn = configuration.GetConnectionString("Postgres")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=MiniERP;Username=postgres;Password=postgres;";
            optionsBuilder.UseNpgsql(conn);
        }
        else
        {
            var conn = configuration.GetConnectionString("DefaultConnection")
                ?? $"Data Source={Path.Combine(basePath, "MiniERP.db")}";
            optionsBuilder.UseSqlite(conn);
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
