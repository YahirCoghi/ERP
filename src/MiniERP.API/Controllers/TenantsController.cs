using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;
using MiniERP.Infrastructure.Services;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.OwnerAdmin)]
public class TenantsController : ControllerBase
{
    private readonly MasterDbContext _masterDb;
    private readonly TenantProvisioningService _provisioning;
    private readonly LicenseService _licenseService;

    public TenantsController(MasterDbContext masterDb, TenantProvisioningService provisioning, LicenseService licenseService)
    {
        _masterDb = masterDb;
        _provisioning = provisioning;
        _licenseService = licenseService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tenants = await _masterDb.Tenants.AsNoTracking().ToListAsync();
        return Ok(tenants);
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await _licenseService.UpdateStatusAsync(id, request.Status, request.PaidUntil);
        return Ok(new { message = "Status updated." });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTenantRequest request)
    {
        var dbName = string.IsNullOrWhiteSpace(request.DatabaseName)
            ? $"MiniERP_{Guid.NewGuid():N}"
            : request.DatabaseName.Trim();

        var connection = _provisioning.BuildTenantConnectionString(dbName);

        var tenant = new Tenant
        {
            Name = request.Name,
            Subdomain = request.Subdomain,
            DatabaseName = dbName,
            ConnectionString = connection,
            Status = request.PaidUntil.HasValue ? SubscriptionStatus.Active : SubscriptionStatus.Trial,
            TrialEndsAt = request.TrialEndsAt,
            PaidUntil = request.PaidUntil,
            MaxUsers = request.MaxUsers,
            PlanCode = request.PlanCode,
            BillingEmail = request.BillingEmail,
            ExternalCustomerId = request.ExternalCustomerId
        };

        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        await _provisioning.CreateTenantDatabaseAsync(tenant);
        await _provisioning.ApplyMigrationsAsync(connection);

        if (!string.IsNullOrWhiteSpace(request.OwnerEmail))
        {
            _masterDb.TenantUsers.Add(new TenantUser
            {
                TenantId = tenant.Id,
                Email = request.OwnerEmail,
                Role = "Owner",
                IsActive = true
            });
            await _masterDb.SaveChangesAsync();
        }

        var license = await _licenseService.CreateAsync(tenant.Id, tenant.PaidUntil ?? tenant.TrialEndsAt, tenant.MaxUsers);

        return Ok(new { tenant, license });
    }

    public record CreateTenantRequest(
        string Name,
        string? Subdomain,
        string? DatabaseName,
        string? OwnerEmail,
        int MaxUsers = 5,
        DateTime? TrialEndsAt = null,
        DateTime? PaidUntil = null,
        string? PlanCode = null,
        string? BillingEmail = null,
        string? ExternalCustomerId = null);

    public record UpdateStatusRequest(SubscriptionStatus Status, DateTime? PaidUntil);
}
