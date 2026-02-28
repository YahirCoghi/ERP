using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;
using MiniERP.Infrastructure.Services;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SignupController : ControllerBase
{
    private readonly MasterDbContext _masterDb;
    private readonly TenantProvisioningService _provisioning;
    private readonly LicenseService _licenseService;

    public SignupController(MasterDbContext masterDb, TenantProvisioningService provisioning, LicenseService licenseService)
    {
        _masterDb = masterDb;
        _provisioning = provisioning;
        _licenseService = licenseService;
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupRequest request)
    {
        var dbName = $"MiniERP_{Guid.NewGuid():N}";
        var connection = _provisioning.BuildTenantConnectionString(dbName);

        var tenant = new Tenant
        {
            Name = request.CompanyName,
            Subdomain = request.Subdomain,
            DatabaseName = dbName,
            ConnectionString = connection,
            Status = SubscriptionStatus.Trial,
            TrialEndsAt = DateTime.UtcNow.AddDays(request.TrialDays),
            MaxUsers = request.MaxUsers,
            PlanCode = request.PlanCode,
            BillingEmail = request.Email
        };

        _masterDb.Tenants.Add(tenant);
        await _masterDb.SaveChangesAsync();

        await _provisioning.CreateTenantDatabaseAsync(tenant);
        await _provisioning.ApplyMigrationsAsync(connection);

        _masterDb.TenantUsers.Add(new TenantUser
        {
            TenantId = tenant.Id,
            Email = request.Email,
            Role = "Owner",
            IsActive = true
        });
        await _masterDb.SaveChangesAsync();

        var license = await _licenseService.CreateAsync(tenant.Id, tenant.TrialEndsAt, tenant.MaxUsers);

        return Ok(new { tenant, license });
    }

    public record SignupRequest(
        string CompanyName,
        string Email,
        string? Subdomain,
        int TrialDays = 14,
        int MaxUsers = 5,
        string? PlanCode = null);
}
