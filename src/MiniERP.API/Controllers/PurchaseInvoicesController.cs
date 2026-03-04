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
public class PurchaseInvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICodeSequenceService _codeSequenceService;

    public PurchaseInvoicesController(ApplicationDbContext context, ICodeSequenceService codeSequenceService)
    {
        _context = context;
        _codeSequenceService = codeSequenceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoice>>> GetPurchaseInvoices()
    {
        return await _context.PurchaseInvoices
            .Include(i => i.Supplier)
            .Include(i => i.Lines)
            .ThenInclude(l => l.Product)
            .Where(i => i.IsActive)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseInvoice>> GetPurchaseInvoice(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(i => i.Supplier)
            .Include(i => i.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);

        if (invoice == null) return NotFound();
        return invoice;
    }

    [HttpPost("from-purchaseorder/{purchaseOrderId:int}")]
    public async Task<ActionResult<PurchaseInvoice>> CreateFromPurchaseOrder(int purchaseOrderId)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId && po.IsActive);

        if (purchaseOrder == null) return NotFound("Purchase order not found.");
        if (purchaseOrder.Status.Equals("Billed", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Purchase order is already billed.");

        var settings = await _context.PurchasingSettings.FirstOrDefaultAsync() ?? new PurchasingSettings();
        var invoice = new PurchaseInvoice
        {
            InvoiceNumber = await _codeSequenceService.GenerateNextAsync("purchase-invoice", "PINV-"),
            InvoiceDate = DateTime.UtcNow,
            SupplierId = purchaseOrder.SupplierId,
            PurchaseOrderId = purchaseOrder.Id,
            Status = "Issued",
            DueDate = DateTime.UtcNow.AddDays(settings.DefaultPaymentTermDays),
            Lines = purchaseOrder.Lines.Select(l => new PurchaseInvoiceLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitCost = l.UnitPrice,
                Discount = l.Discount
            }).ToList()
        };

        ApplyInvoiceTotals(invoice, settings.DefaultTaxRate);
        invoice.PaidAmount = 0m;
        invoice.Balance = invoice.Total;
        _context.PurchaseInvoices.Add(invoice);

        purchaseOrder.Status = "Billed";
        purchaseOrder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPurchaseInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var invoice = await _context.PurchaseInvoices.FindAsync(id);
        if (invoice == null || !invoice.IsActive) return NotFound();
        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> RegisterPayment(int id, [FromBody] RegisterPaymentRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest(new { message = "Payment amount must be greater than zero." });

        var invoice = await _context.PurchaseInvoices.FindAsync(id);
        if (invoice == null || !invoice.IsActive) return NotFound();

        if (invoice.Balance <= 0)
            return BadRequest(new { message = "Invoice is already fully paid." });

        if (request.Amount > invoice.Balance)
            return BadRequest(new { message = "Payment amount cannot exceed invoice balance." });

        invoice.PaidAmount += request.Amount;
        invoice.Balance = invoice.Total - invoice.PaidAmount;
        invoice.LastPaymentDate = request.PaymentDate ?? DateTime.UtcNow;
        invoice.Status = invoice.Balance <= 0m ? "Paid" : "Partial";
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new
        {
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Total,
            invoice.PaidAmount,
            invoice.Balance,
            invoice.Status,
            invoice.LastPaymentDate
        });
    }

    private static void ApplyInvoiceTotals(PurchaseInvoice invoice, decimal taxRate)
    {
        decimal subtotal = 0;
        foreach (var line in invoice.Lines)
        {
            var lineSubtotal = line.Quantity * line.UnitCost - line.Discount;
            line.Total = lineSubtotal;
            subtotal += lineSubtotal;
        }

        var taxAmount = subtotal * (taxRate / 100m);
        invoice.Subtotal = subtotal;
        invoice.Tax = taxAmount;
        invoice.Total = subtotal + taxAmount;
    }

    public record RegisterPaymentRequest(decimal Amount, DateTime? PaymentDate = null);
}
