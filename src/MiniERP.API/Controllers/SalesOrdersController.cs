using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Sales)]
public class SalesOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SalesOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> GetSalesOrders()
    {
        return await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Lines)
            .ThenInclude(l => l.Product)
            .Where(so => so.IsActive)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SalesOrder>> GetSalesOrder(int id)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(so => so.Id == id);
        if (salesOrder == null) return NotFound();
        return salesOrder;
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrder>> CreateSalesOrder(SalesOrder salesOrder)
    {
        var settings = await _context.SalesSettings.FirstOrDefaultAsync() ?? new SalesSettings();
        salesOrder.OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        salesOrder.CreatedAt = DateTime.UtcNow;
        salesOrder.Currency = salesOrder.Currency == 0 ? settings.DefaultCurrency : salesOrder.Currency;
        salesOrder.Status = string.IsNullOrWhiteSpace(salesOrder.Status) ? "Pending" : salesOrder.Status;
        ApplySalesOrderTotals(salesOrder, settings.DefaultTaxRate);
        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSalesOrder), new { id = salesOrder.Id }, salesOrder);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var salesOrder = await _context.SalesOrders.FindAsync(id);
        if (salesOrder == null) return NotFound();
        salesOrder.Status = status;
        salesOrder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static void ApplySalesOrderTotals(SalesOrder salesOrder, decimal taxRate)
    {
        decimal subtotal = 0;
        foreach (var line in salesOrder.Lines)
        {
            var lineSubtotal = line.Quantity * line.UnitPrice - line.Discount;
            line.Total = lineSubtotal;
            subtotal += lineSubtotal;
        }

        var taxAmount = subtotal * (taxRate / 100m);
        salesOrder.Subtotal = subtotal;
        salesOrder.Tax = taxAmount;
        salesOrder.Total = subtotal + taxAmount;
    }
}
