using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Purchasing;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Purchasing)]
public class PurchaseOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICodeSequenceService _codeSequenceService;

    public PurchaseOrdersController(ApplicationDbContext context, ICodeSequenceService codeSequenceService)
    {
        _context = context;
        _codeSequenceService = codeSequenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetPurchaseOrders()
    {
        return await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Lines)
            .ThenInclude(l => l.Product)
            .Where(po => po.IsActive)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrder>> GetPurchaseOrder(int id)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(po => po.Id == id);
        if (purchaseOrder == null) return NotFound();
        return purchaseOrder;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrder>> CreatePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.SupplierId <= 0)
            return BadRequest(new { message = "Supplier is required." });

        if (purchaseOrder.Lines == null || purchaseOrder.Lines.Count == 0)
            return BadRequest(new { message = "At least one purchase order line is required." });

        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == purchaseOrder.SupplierId && s.IsActive);
        if (!supplierExists)
            return BadRequest(new { message = "Selected supplier does not exist or is inactive." });

        var productIds = purchaseOrder.Lines
            .Select(l => l.ProductId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (productIds.Count != purchaseOrder.Lines.Count)
            return BadRequest(new { message = "Every line must have a valid product." });

        var validProductIds = await _context.Products
            .Where(p => p.IsActive && productIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var invalidProductIds = productIds.Except(validProductIds).ToList();
        if (invalidProductIds.Count > 0)
            return BadRequest(new { message = $"Invalid or inactive product(s): {string.Join(", ", invalidProductIds)}." });

        if (purchaseOrder.Lines.Any(l => l.Quantity <= 0 || l.UnitPrice <= 0))
            return BadRequest(new { message = "Line quantities and unit prices must be greater than zero." });

        try
        {
            var settings = await _context.PurchasingSettings.FirstOrDefaultAsync() ?? new PurchasingSettings();
            purchaseOrder.OrderNumber = await _codeSequenceService.GenerateNextAsync("purchase-order", "PO-");
            purchaseOrder.CreatedAt = DateTime.UtcNow;
            purchaseOrder.Currency = purchaseOrder.Currency == 0 ? settings.DefaultCurrency : purchaseOrder.Currency;
            purchaseOrder.Status = string.IsNullOrWhiteSpace(purchaseOrder.Status) ? "Pending" : purchaseOrder.Status;
            ApplyPurchaseOrderTotals(purchaseOrder, settings.DefaultTaxRate);
            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.Id }, purchaseOrder);
        }
        catch (DbUpdateException ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { message = "Could not save purchase order.", detail });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
        if (purchaseOrder == null) return NotFound();
        purchaseOrder.Status = status;
        purchaseOrder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/receive")]
    public async Task<IActionResult> Receive(int id, [FromBody] ReceivePurchaseOrderRequest request)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == id);

        if (purchaseOrder == null) return NotFound();
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest(new { message = "At least one receipt line is required." });

        var orderedLines = purchaseOrder.Lines.ToDictionary(l => l.ProductId, l => l);
        var unknownProducts = request.Lines.Where(l => !orderedLines.ContainsKey(l.ProductId)).Select(l => l.ProductId).Distinct().ToList();
        if (unknownProducts.Count > 0)
            return BadRequest(new { message = $"Products not present in order: {string.Join(", ", unknownProducts)}." });

        var invalidQty = request.Lines.FirstOrDefault(l => l.Quantity <= 0 || l.UnitCost <= 0);
        if (invalidQty != null)
            return BadRequest(new { message = "Received quantities and unit costs must be greater than zero." });

        var exceeded = request.Lines
            .Where(l => l.Quantity > orderedLines[l.ProductId].Quantity)
            .Select(l => l.ProductId)
            .Distinct()
            .ToList();

        if (exceeded.Count > 0)
            return BadRequest(new { message = $"Received quantity cannot exceed ordered quantity for product(s): {string.Join(", ", exceeded)}." });

        foreach (var line in request.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null) continue;

            product.Stock += (int)line.Quantity;

            _context.InventoryMovements.Add(new Domain.Entities.Inventory.InventoryMovement
            {
                ProductId = line.ProductId,
                Type = "IN",
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                TotalCost = line.Quantity * line.UnitCost,
                Reference = purchaseOrder.OrderNumber,
                ReferenceId = purchaseOrder.Id,
                Notes = $"Recepcion PO {purchaseOrder.OrderNumber}"
            });
        }

        purchaseOrder.Status = request.IsFinal ? "Received" : "PartiallyReceived";
        purchaseOrder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Receipt posted.", purchaseOrder.Status });
    }

    public record ReceivePurchaseOrderLine(int ProductId, decimal Quantity, decimal UnitCost);
    public record ReceivePurchaseOrderRequest(bool IsFinal, List<ReceivePurchaseOrderLine> Lines);

    private static void ApplyPurchaseOrderTotals(PurchaseOrder purchaseOrder, decimal taxRate)
    {
        decimal subtotal = 0;
        foreach (var line in purchaseOrder.Lines)
        {
            var lineSubtotal = line.Quantity * line.UnitPrice - line.Discount;
            line.Total = lineSubtotal;
            subtotal += lineSubtotal;
        }

        var taxAmount = subtotal * (taxRate / 100m);
        purchaseOrder.Subtotal = subtotal;
        purchaseOrder.Tax = taxAmount;
        purchaseOrder.Total = subtotal + taxAmount;
    }
}
