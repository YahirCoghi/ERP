using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class TenantProvisioningService
{
    private readonly TenantProvisioningOptions _options;

    public TenantProvisioningService(IOptions<TenantProvisioningOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Tenant> CreateTenantDatabaseAsync(Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(_options.AdminConnectionString))
        {
            throw new InvalidOperationException("AdminConnectionString is required for provisioning.");
        }

        using var connection = new SqlConnection(_options.AdminConnectionString);
        await connection.OpenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = $"IF DB_ID('{tenant.DatabaseName}') IS NULL CREATE DATABASE [{tenant.DatabaseName}]";
        await cmd.ExecuteNonQueryAsync();

        return tenant;
    }

    public async Task ApplyMigrationsAsync(string tenantConnectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(tenantConnectionString);
        await using var ctx = new ApplicationDbContext(optionsBuilder.Options);
        await ctx.Database.MigrateAsync();
    }

    public string BuildTenantConnectionString(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(_options.TenantConnectionTemplate))
        {
            throw new InvalidOperationException("TenantConnectionTemplate is required.");
        }

        return _options.TenantConnectionTemplate.Replace("{db}", databaseName, StringComparison.OrdinalIgnoreCase);
    }
}
