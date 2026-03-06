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
[Authorize(Policy = "Addons:Read")]
public class AddonsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AddonsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions()
    {
        var rows = await _db.AddonDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new AddonDefinitionDto(x.Id, x.Code, x.Name, x.Version, x.ConfigurationSchemaJson))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("definitions")]
    [Authorize(Policy = "Addons:Full")]
    public async Task<IActionResult> CreateDefinition([FromBody] AddonDefinitionDto request)
    {
        var row = new AddonDefinition
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Version = string.IsNullOrWhiteSpace(request.Version) ? "1.0.0" : request.Version.Trim(),
            ConfigurationSchemaJson = request.ConfigurationSchemaJson
        };
        _db.AddonDefinitions.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new AddonDefinitionDto(row.Id, row.Code, row.Name, row.Version, row.ConfigurationSchemaJson));
    }

    [HttpGet("activations")]
    public async Task<IActionResult> GetActivations()
    {
        var rows = await _db.AddonActivations
            .AsNoTracking()
            .Include(x => x.AddonDefinition)
            .Where(x => x.IsActive)
            .OrderBy(x => x.AddonDefinition!.Code)
            .Select(x => new
            {
                x.Id,
                x.AddonDefinitionId,
                AddonCode = x.AddonDefinition!.Code,
                AddonName = x.AddonDefinition!.Name,
                x.Enabled,
                x.ConfigurationJson,
                x.EnabledAt
            })
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("activations")]
    [Authorize(Policy = "Addons:Full")]
    public async Task<IActionResult> UpsertActivation([FromBody] AddonActivationDto request)
    {
        var addonExists = await _db.AddonDefinitions.AnyAsync(x => x.Id == request.AddonDefinitionId && x.IsActive);
        if (!addonExists)
            return BadRequest(new { message = "Addon definition not found." });

        var row = await _db.AddonActivations.FirstOrDefaultAsync(x => x.AddonDefinitionId == request.AddonDefinitionId);
        if (row == null)
        {
            row = new AddonActivation
            {
                AddonDefinitionId = request.AddonDefinitionId,
                Enabled = request.Enabled,
                ConfigurationJson = request.ConfigurationJson,
                EnabledAt = request.Enabled ? DateTime.UtcNow : null
            };
            _db.AddonActivations.Add(row);
        }
        else
        {
            row.Enabled = request.Enabled;
            row.ConfigurationJson = request.ConfigurationJson;
            row.EnabledAt = request.Enabled ? DateTime.UtcNow : null;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new AddonActivationDto(row.Id, row.AddonDefinitionId, row.Enabled, row.ConfigurationJson, row.EnabledAt));
    }

    [HttpGet("mobile")]
    [Authorize(Policy = "Mobile:Read")]
    public async Task<IActionResult> GetMobileConfig()
    {
        var row = await _db.MobileServiceConfigs.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        if (row == null)
            return Ok(null);
        return Ok(new MobileServiceConfigDto(row.Id, row.Provider, row.MaxDevices, row.Enabled, row.LicenseExpiresAt));
    }

    [HttpPost("mobile")]
    [Authorize(Policy = "Mobile:Full")]
    public async Task<IActionResult> UpsertMobileConfig([FromBody] MobileServiceConfigDto request)
    {
        var row = await _db.MobileServiceConfigs.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive);
        if (row == null)
        {
            row = new MobileServiceConfig
            {
                Provider = request.Provider.Trim(),
                MaxDevices = request.MaxDevices,
                Enabled = request.Enabled,
                LicenseExpiresAt = request.LicenseExpiresAt
            };
            _db.MobileServiceConfigs.Add(row);
        }
        else
        {
            row.Provider = request.Provider.Trim();
            row.MaxDevices = request.MaxDevices;
            row.Enabled = request.Enabled;
            row.LicenseExpiresAt = request.LicenseExpiresAt;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new MobileServiceConfigDto(row.Id, row.Provider, row.MaxDevices, row.Enabled, row.LicenseExpiresAt));
    }
}
