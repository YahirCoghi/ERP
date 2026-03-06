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
[Authorize(Roles = AppRoles.Operations)]
public class ProductionAdvancedController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductionAdvancedController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("procurement-confirmations")]
    [Authorize(Policy = "ProcurementWizard:Read")]
    public async Task<IActionResult> GetProcurementConfirmations()
    {
        var rows = await _db.ProcurementConfirmations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProcurementConfirmationDto(x.Id, x.ReferenceType, x.ReferenceId, x.Status, x.IsPartial, x.Notes))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("procurement-confirmations")]
    [Authorize(Policy = "ProcurementWizard:Full")]
    public async Task<IActionResult> CreateProcurementConfirmation([FromBody] ProcurementConfirmationDto request)
    {
        var row = new ProcurementConfirmation
        {
            ReferenceType = request.ReferenceType.Trim(),
            ReferenceId = request.ReferenceId,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim(),
            IsPartial = request.IsPartial,
            Notes = request.Notes
        };
        _db.ProcurementConfirmations.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new ProcurementConfirmationDto(row.Id, row.ReferenceType, row.ReferenceId, row.Status, row.IsPartial, row.Notes));
    }

    [HttpPost("procurement-confirmations/{id:int}/lines")]
    [Authorize(Policy = "ProcurementWizard:Full")]
    public async Task<IActionResult> AddProcurementLine(int id, [FromBody] ProcurementConfirmationLineDto request)
    {
        var headerExists = await _db.ProcurementConfirmations.AnyAsync(x => x.Id == id && x.IsActive);
        if (!headerExists)
            return NotFound(new { message = "Procurement confirmation not found." });

        var row = new ProcurementConfirmationLine
        {
            ProcurementConfirmationId = id,
            ProductId = request.ProductId,
            RequestedQty = request.RequestedQty,
            ConfirmedQty = request.ConfirmedQty
        };
        _db.ProcurementConfirmationLines.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new ProcurementConfirmationLineDto(row.Id, row.ProcurementConfirmationId, row.ProductId, row.RequestedQty, row.ConfirmedQty));
    }

    [HttpPost("procurement-confirmations/{id:int}/confirm")]
    [Authorize(Policy = "ProcurementWizard:Full")]
    public async Task<IActionResult> ConfirmProcurement(int id)
    {
        var header = await _db.ProcurementConfirmations.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (header == null)
            return NotFound(new { message = "Procurement confirmation not found." });

        header.Status = "Confirmed";
        header.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new ProcurementConfirmationDto(header.Id, header.ReferenceType, header.ReferenceId, header.Status, header.IsPartial, header.Notes));
    }

    [HttpGet("pick-pack")]
    [Authorize(Policy = "PickPack:Read")]
    public async Task<IActionResult> GetPickPackTasks()
    {
        var rows = await _db.PickPackTasks
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PickPackTaskDto(x.Id, x.TaskNumber, x.TaskType, x.Status, x.Warehouse))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("pick-pack")]
    [Authorize(Policy = "PickPack:Full")]
    public async Task<IActionResult> CreatePickPackTask([FromBody] PickPackTaskDto request)
    {
        var row = new PickPackTask
        {
            TaskNumber = request.TaskNumber.Trim(),
            TaskType = request.TaskType.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            Warehouse = request.Warehouse
        };
        _db.PickPackTasks.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new PickPackTaskDto(row.Id, row.TaskNumber, row.TaskType, row.Status, row.Warehouse));
    }

    [HttpPost("pick-pack/{id:int}/lines")]
    [Authorize(Policy = "PickPack:Full")]
    public async Task<IActionResult> AddPickPackLine(int id, [FromBody] PickPackTaskLineDto request)
    {
        var taskExists = await _db.PickPackTasks.AnyAsync(x => x.Id == id && x.IsActive);
        if (!taskExists)
            return NotFound(new { message = "Pick/Pack task not found." });

        var row = new PickPackTaskLine
        {
            PickPackTaskId = id,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            ConfirmedQuantity = request.ConfirmedQuantity
        };
        _db.PickPackTaskLines.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new PickPackTaskLineDto(row.Id, row.PickPackTaskId, row.ProductId, row.Quantity, row.ConfirmedQuantity));
    }

    [HttpPost("pick-pack/{id:int}/complete")]
    [Authorize(Policy = "PickPack:Full")]
    public async Task<IActionResult> CompletePickPack(int id)
    {
        var row = await _db.PickPackTasks.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (row == null)
            return NotFound(new { message = "Pick/Pack task not found." });

        row.Status = "Completed";
        row.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new PickPackTaskDto(row.Id, row.TaskNumber, row.TaskType, row.Status, row.Warehouse));
    }
}
