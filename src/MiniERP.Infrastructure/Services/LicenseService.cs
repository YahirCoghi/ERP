using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class LicenseService
{
    private readonly MasterDbContext _masterDb;

    public LicenseService(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    public async Task<License> CreateAsync(int tenantId, DateTime? expiresAt, int maxUsers)
    {
        var key = $"LIC-{Guid.NewGuid():N}";
        var license = new License
        {
            TenantId = tenantId,
            LicenseKey = key,
            Status = SubscriptionStatus.Active,
            ExpiresAt = expiresAt,
            MaxUsers = maxUsers
        };
        _masterDb.Licenses.Add(license);
        await _masterDb.SaveChangesAsync();
        return license;
    }

    public async Task<License?> GetByKeyAsync(string key)
    {
        return await _masterDb.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == key);
    }

    public async Task UpdateStatusAsync(int tenantId, SubscriptionStatus status, DateTime? paidUntil)
    {
        var tenant = await _masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null) return;

        tenant.Status = status;
        tenant.PaidUntil = paidUntil;
        tenant.UpdatedAt = DateTime.UtcNow;

        var license = await _masterDb.Licenses.FirstOrDefaultAsync(l => l.TenantId == tenantId);
        if (license != null)
        {
            license.Status = status;
            license.ExpiresAt = paidUntil;
            license.UpdatedAt = DateTime.UtcNow;
        }

        await _masterDb.SaveChangesAsync();
    }
}
