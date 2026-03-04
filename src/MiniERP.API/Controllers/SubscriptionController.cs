using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Contracts;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly MasterDbContext _masterDb;
    private readonly ApplicationDbContext _appDb;
    private readonly ITenantAccessor _tenantAccessor;

    public SubscriptionController(MasterDbContext masterDb, ApplicationDbContext appDb, ITenantAccessor tenantAccessor)
    {
        _masterDb = masterDb;
        _appDb = appDb;
        _tenantAccessor = tenantAccessor;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var tenantId = _tenantAccessor.TenantId;
        if (tenantId <= 0)
            return BadRequest(new { message = "Tenant context is required." });

        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

        if (tenant == null)
            return NotFound(new { message = "Tenant not found." });

        var license = await _masterDb.Licenses
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.TenantId == tenantId);

        var activeUsers = await _appDb.Users.CountAsync(u => u.IsActive);
        var remainingUsers = Math.Max(0, tenant.MaxUsers - activeUsers);

        return Ok(new
        {
            tenant.Id,
            tenant.Name,
            tenant.Status,
            tenant.PlanCode,
            tenant.TrialEndsAt,
            tenant.PaidUntil,
            tenant.MaxUsers,
            ActiveUsers = activeUsers,
            RemainingUsers = remainingUsers,
            License = license == null
                ? null
                : new
                {
                    license.Id,
                    license.LicenseKey,
                    license.Status,
                    license.ExpiresAt
                }
        });
    }
}
