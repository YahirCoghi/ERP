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
[Authorize(Policy = "WorkflowManager:Read")]
public class WorkflowManagerController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public WorkflowManagerController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions()
    {
        var rows = await _db.WorkflowDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new WorkflowDefinitionDto(x.Id, x.Code, x.Name, x.Module, x.IsPublished))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("definitions")]
    [Authorize(Policy = "WorkflowManager:Full")]
    public async Task<IActionResult> CreateDefinition([FromBody] WorkflowDefinitionDto request)
    {
        var row = new WorkflowDefinition
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Module = request.Module.Trim(),
            IsPublished = request.IsPublished
        };
        _db.WorkflowDefinitions.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new WorkflowDefinitionDto(row.Id, row.Code, row.Name, row.Module, row.IsPublished));
    }

    [HttpGet("definitions/{id:int}/steps")]
    public async Task<IActionResult> GetSteps(int id)
    {
        var rows = await _db.WorkflowSteps
            .AsNoTracking()
            .Where(x => x.WorkflowDefinitionId == id && x.IsActive)
            .OrderBy(x => x.StepOrder)
            .Select(x => new WorkflowStepDto(x.Id, x.WorkflowDefinitionId, x.StepOrder, x.Name, x.ActionType, x.ConditionsJson))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("definitions/{id:int}/steps")]
    [Authorize(Policy = "WorkflowManager:Full")]
    public async Task<IActionResult> AddStep(int id, [FromBody] WorkflowStepDto request)
    {
        var definitionExists = await _db.WorkflowDefinitions.AnyAsync(x => x.Id == id && x.IsActive);
        if (!definitionExists)
            return NotFound(new { message = "Workflow definition not found." });

        var row = new WorkflowStep
        {
            WorkflowDefinitionId = id,
            StepOrder = request.StepOrder,
            Name = request.Name.Trim(),
            ActionType = request.ActionType.Trim(),
            ConditionsJson = request.ConditionsJson
        };
        _db.WorkflowSteps.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new WorkflowStepDto(row.Id, row.WorkflowDefinitionId, row.StepOrder, row.Name, row.ActionType, row.ConditionsJson));
    }

    [HttpGet("instances")]
    public async Task<IActionResult> GetInstances()
    {
        var rows = await _db.WorkflowInstances
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.StartedAt)
            .Take(500)
            .Select(x => new WorkflowInstanceDto(x.Id, x.WorkflowDefinitionId, x.ReferenceType, x.ReferenceId, x.Status, x.StartedAt, x.CompletedAt))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("instances")]
    [Authorize(Policy = "WorkflowManager:Full")]
    public async Task<IActionResult> StartInstance([FromBody] WorkflowInstanceDto request)
    {
        var definition = await _db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == request.WorkflowDefinitionId && x.IsActive);
        if (definition == null || !definition.IsPublished)
            return BadRequest(new { message = "Workflow definition is not available or not published." });

        var row = new WorkflowInstance
        {
            WorkflowDefinitionId = request.WorkflowDefinitionId,
            ReferenceType = request.ReferenceType.Trim(),
            ReferenceId = request.ReferenceId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Running" : request.Status.Trim(),
            StartedAt = DateTime.UtcNow
        };
        _db.WorkflowInstances.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new WorkflowInstanceDto(row.Id, row.WorkflowDefinitionId, row.ReferenceType, row.ReferenceId, row.Status, row.StartedAt, row.CompletedAt));
    }

    [HttpPost("instances/{id:int}/complete")]
    [Authorize(Policy = "WorkflowManager:Full")]
    public async Task<IActionResult> CompleteInstance(int id)
    {
        var row = await _db.WorkflowInstances.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (row == null)
            return NotFound(new { message = "Workflow instance not found." });

        row.Status = "Completed";
        row.CompletedAt = DateTime.UtcNow;
        row.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new WorkflowInstanceDto(row.Id, row.WorkflowDefinitionId, row.ReferenceType, row.ReferenceId, row.Status, row.StartedAt, row.CompletedAt));
    }
}
