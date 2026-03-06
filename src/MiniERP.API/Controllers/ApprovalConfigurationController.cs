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
[Authorize(Policy = "Approvals:Read")]
public class ApprovalConfigurationController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApprovalConfigurationController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var rows = await _db.ApprovalTemplates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .Select(x => new ApprovalTemplateDto(x.Id, x.Code, x.Name, x.Module, x.Description))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("templates")]
    [Authorize(Policy = "Approvals:Full")]
    public async Task<IActionResult> CreateTemplate([FromBody] ApprovalTemplateDto request)
    {
        var row = new ApprovalTemplate
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Module = request.Module.Trim(),
            Description = request.Description
        };
        _db.ApprovalTemplates.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new ApprovalTemplateDto(row.Id, row.Code, row.Name, row.Module, row.Description));
    }

    [HttpGet("templates/{id:int}/stages")]
    public async Task<IActionResult> GetStages(int id)
    {
        var rows = await _db.ApprovalTemplateStages
            .AsNoTracking()
            .Where(x => x.TemplateId == id && x.IsActive)
            .OrderBy(x => x.StageOrder)
            .Select(x => new ApprovalTemplateStageDto(x.Id, x.TemplateId, x.StageOrder, x.Name, x.RoleRequired, x.MinAmount))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("templates/{id:int}/stages")]
    [Authorize(Policy = "Approvals:Full")]
    public async Task<IActionResult> AddStage(int id, [FromBody] ApprovalTemplateStageDto request)
    {
        var templateExists = await _db.ApprovalTemplates.AnyAsync(x => x.Id == id && x.IsActive);
        if (!templateExists)
            return NotFound(new { message = "Approval template not found." });

        var row = new ApprovalTemplateStage
        {
            TemplateId = id,
            StageOrder = request.StageOrder,
            Name = request.Name.Trim(),
            RoleRequired = request.RoleRequired.Trim(),
            MinAmount = request.MinAmount
        };
        _db.ApprovalTemplateStages.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new ApprovalTemplateStageDto(row.Id, row.TemplateId, row.StageOrder, row.Name, row.RoleRequired, row.MinAmount));
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetApprovalReport([FromQuery] int days = 30)
    {
        var minDate = DateTime.UtcNow.AddDays(-Math.Abs(days));
        var rows = await _db.ApprovalRequests
            .AsNoTracking()
            .Where(x => x.IsActive && x.CreatedAt >= minDate)
            .ToListAsync();

        var report = new
        {
            PeriodDays = Math.Abs(days),
            Total = rows.Count,
            Pending = rows.Count(x => x.Status == "Pending"),
            Approved = rows.Count(x => x.Status == "Approved"),
            Rejected = rows.Count(x => x.Status == "Rejected"),
            ByModule = rows
                .GroupBy(x => x.Module)
                .Select(g => new { Module = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList()
        };
        return Ok(report);
    }
}
