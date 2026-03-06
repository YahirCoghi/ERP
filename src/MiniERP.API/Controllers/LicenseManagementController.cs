using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Application.DTOs;
using MiniERP.Domain.Entities.Advanced;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.OwnerAdmin)]
[Authorize(Policy = "LicenseManagement:Read")]
public class LicenseManagementController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LicenseManagementController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var rows = await _db.TenantLicenseAssignments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.LicenseCode)
            .Select(x => new TenantLicenseAssignmentDto(x.Id, x.LicenseCode, x.Scope, x.UserId, x.Enabled, x.ExpiresAt))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Policy = "LicenseManagement:Full")]
    public async Task<IActionResult> Create([FromBody] TenantLicenseAssignmentDto request)
    {
        var row = new TenantLicenseAssignment
        {
            LicenseCode = request.LicenseCode.Trim(),
            Scope = string.IsNullOrWhiteSpace(request.Scope) ? "Tenant" : request.Scope.Trim(),
            UserId = request.UserId,
            Enabled = request.Enabled,
            ExpiresAt = request.ExpiresAt
        };
        _db.TenantLicenseAssignments.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new TenantLicenseAssignmentDto(row.Id, row.LicenseCode, row.Scope, row.UserId, row.Enabled, row.ExpiresAt));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "LicenseManagement:Full")]
    public async Task<IActionResult> Update(int id, [FromBody] TenantLicenseAssignmentDto request)
    {
        var row = await _db.TenantLicenseAssignments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (row == null)
            return NotFound();

        row.LicenseCode = request.LicenseCode.Trim();
        row.Scope = string.IsNullOrWhiteSpace(request.Scope) ? "Tenant" : request.Scope.Trim();
        row.UserId = request.UserId;
        row.Enabled = request.Enabled;
        row.ExpiresAt = request.ExpiresAt;
        row.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new TenantLicenseAssignmentDto(row.Id, row.LicenseCode, row.Scope, row.UserId, row.Enabled, row.ExpiresAt));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "LicenseManagement:Full")]
    public async Task<IActionResult> Delete(int id)
    {
        var row = await _db.TenantLicenseAssignments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (row == null)
            return NotFound();

        row.IsActive = false;
        row.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
