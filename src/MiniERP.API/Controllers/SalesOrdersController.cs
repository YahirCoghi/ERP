using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Sales)]
public class SalesOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICodeSequenceService _codeSequenceService;

    public SalesOrdersController(ApplicationDbContext context, ICodeSequenceService codeSequenceService)
    {
        _context = context;
        _codeSequenceService = codeSequenceService;
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
        if (salesOrder.CustomerId <= 0)
            return BadRequest(new { message = "Customer is required." });

        if (salesOrder.Lines == null || salesOrder.Lines.Count == 0)
            return BadRequest(new { message = "At least one sales order line is required." });

        var customerExists = await _context.Customers.AnyAsync(c => c.Id == salesOrder.CustomerId && c.IsActive);
        if (!customerExists)
            return BadRequest(new { message = "Selected customer does not exist or is inactive." });

        var productIds = salesOrder.Lines
            .Select(l => l.ProductId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (productIds.Count != salesOrder.Lines.Count)
            return BadRequest(new { message = "Every line must have a valid product." });

        var validProductIds = await _context.Products
            .Where(p => p.IsActive && productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var invalidProductIds = productIds.Except(validProductIds).ToList();
        if (invalidProductIds.Count > 0)
            return BadRequest(new { message = $"Invalid or inactive product(s): {string.Join(", ", invalidProductIds)}." });

        try
        {
            var settings = await _context.SalesSettings.FirstOrDefaultAsync() ?? new SalesSettings();
            salesOrder.OrderNumber = await _codeSequenceService.GenerateNextAsync("sales-order", "SO-");
            salesOrder.CreatedAt = DateTime.UtcNow;
            salesOrder.Currency = salesOrder.Currency == 0 ? settings.DefaultCurrency : salesOrder.Currency;
            salesOrder.Status = string.IsNullOrWhiteSpace(salesOrder.Status) ? "Pending" : salesOrder.Status;
            ApplySalesOrderTotals(salesOrder, settings.DefaultTaxRate);
            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            var response = new
            {
                salesOrder.Id,
                salesOrder.OrderNumber,
                salesOrder.OrderDate,
                salesOrder.CustomerId,
                salesOrder.Subtotal,
                salesOrder.Tax,
                salesOrder.Total,
                salesOrder.Currency,
                salesOrder.Status,
                Lines = salesOrder.Lines.Select(l => new
                {
                    l.Id,
                    l.ProductId,
                    l.Quantity,
                    l.UnitPrice,
                    l.Discount,
                    l.Total
                })
            };

            return CreatedAtAction(nameof(GetSalesOrder), new { id = salesOrder.Id }, response);
        }
        catch (DbUpdateException ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { message = "Could not save sales order.", detail });
        }
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
