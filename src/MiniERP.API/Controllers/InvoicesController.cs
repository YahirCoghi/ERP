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

[Authorize(Roles = AppRoles.Accounting)]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .ThenInclude(l => l.Product)
            .Where(i => i.IsActive)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoice(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return invoice;
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
    {
        invoice.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        invoice.InvoiceDate = DateTime.UtcNow;
        await ApplyLineTotalsAsync(invoice);
        invoice.CreatedAt = DateTime.UtcNow;

        _context.Invoices.Add(invoice);

        // Actualizar stock y crear movimiento de inventario
        foreach (var line in invoice.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product != null)
            {
                product.Stock -= (int)line.Quantity;
                
                _context.InventoryMovements.Add(new Domain.Entities.Inventory.InventoryMovement
                {
                    ProductId = line.ProductId,
                    Type = "OUT",
                    Quantity = (int)line.Quantity,
                    UnitCost = product.Cost,
                    TotalCost = line.Quantity * product.Cost,
                    Reference = invoice.InvoiceNumber,
                    ReferenceId = invoice.Id,
                    Notes = $"Venta - Factura {invoice.InvoiceNumber}"
                });
            }
        }

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPost("from-salesorder/{salesOrderId}")]
    public async Task<ActionResult<Invoice>> CreateFromSalesOrder(int salesOrderId)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(so => so.Id == salesOrderId);

        if (salesOrder == null) return NotFound("Orden de venta no encontrada");

        var settings = await _context.SalesSettings.FirstOrDefaultAsync() ?? new SalesSettings();
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = salesOrder.CustomerId,
            SalesOrderId = salesOrderId,
            Status = "Pending",
            SaleCondition = settings.DefaultSaleCondition,
            PaymentMethod = settings.DefaultPaymentMethod,
            DueDate = DateTime.UtcNow.AddDays(settings.DefaultCreditDays),
            Lines = salesOrder.Lines.Select(l => new InvoiceLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = l.Discount,
                Total = l.Total
            }).ToList()
        };

        await ApplyLineTotalsAsync(invoice);
        _context.Invoices.Add(invoice);

        // Actualizar stock y crear movimientos
        foreach (var line in invoice.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product != null)
            {
                product.Stock -= (int)line.Quantity;
                
                _context.InventoryMovements.Add(new Domain.Entities.Inventory.InventoryMovement
                {
                    ProductId = line.ProductId,
                    Type = "OUT",
                    Quantity = (int)line.Quantity,
                    UnitCost = product.Cost,
                    TotalCost = line.Quantity * product.Cost,
                    Reference = invoice.InvoiceNumber,
                    ReferenceId = invoice.Id,
                    Notes = $"Venta desde OV {salesOrder.OrderNumber}"
                });
            }
        }

        salesOrder.Status = "Invoiced";
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/credit-note")]
    public async Task<ActionResult<Invoice>> CreateCreditNote(int id, [FromBody] CreditNoteRequest request)
    {
        var original = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (original == null)
        {
            return NotFound("Factura original no encontrada");
        }

        var originalElectronic = await _context.ElectronicInvoices
            .FirstOrDefaultAsync(e => e.InvoiceId == original.Id && e.DocumentType == "01");

        var referenceNumber = originalElectronic?.Consecutive ?? original.InvoiceNumber;

        var creditNote = new Invoice
        {
            InvoiceNumber = $"NC-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = original.CustomerId,
            Subtotal = 0,
            Tax = 0,
            Total = 0,
            Status = "Pending",
            ReferenceDocumentType = "01",
            ReferenceNumber = referenceNumber,
            ReferenceDate = original.InvoiceDate,
            ReferenceCode = request.Code,
            ReferenceReason = request.Reason,
            Lines = request.Lines?.Count > 0
                ? request.Lines.Select(l => new InvoiceLine
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Discount = l.Discount
                }).ToList()
                : original.Lines.Select(l => new InvoiceLine
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Discount = l.Discount
                }).ToList()
        };

        await ApplyLineTotalsAsync(creditNote);
        _context.Invoices.Add(creditNote);

        if (request.Restock)
        {
            foreach (var line in creditNote.Lines)
            {
                var product = await _context.Products.FindAsync(line.ProductId);
                if (product != null)
                {
                    product.Stock += (int)line.Quantity;
                    _context.InventoryMovements.Add(new Domain.Entities.Inventory.InventoryMovement
                    {
                        ProductId = line.ProductId,
                        Type = "IN",
                        Quantity = (int)line.Quantity,
                        UnitCost = product.Cost,
                        TotalCost = line.Quantity * product.Cost,
                        Reference = creditNote.InvoiceNumber,
                        ReferenceId = creditNote.Id,
                        Notes = $"NC - Devolucion {creditNote.InvoiceNumber}"
                    });
                }
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInvoice), new { id = creditNote.Id }, creditNote);
    }

    private async Task ApplyLineTotalsAsync(Invoice invoice)
    {
        decimal subtotal = 0;
        decimal taxTotal = 0;

        foreach (var line in invoice.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            var lineSubtotal = line.Quantity * line.UnitPrice - line.Discount;
            var taxRate = line.TaxRate > 0 ? line.TaxRate : (product?.TaxRate ?? 0m);

            if (product?.IsTaxExempt == true)
            {
                taxRate = 0m;
            }

            var lineTax = line.TaxAmount > 0 ? line.TaxAmount : lineSubtotal * taxRate / 100m;
            line.TaxRate = taxRate;
            line.TaxAmount = lineTax;
            line.Total = lineSubtotal + lineTax;

            subtotal += lineSubtotal;
            taxTotal += lineTax;
        }

        invoice.Subtotal = subtotal;
        invoice.Tax = taxTotal;
        invoice.Total = subtotal + taxTotal;
    }

    public record CreditNoteLineRequest(int ProductId, decimal Quantity, decimal UnitPrice, decimal Discount);
    public record CreditNoteRequest(string Reason, string Code = "01", bool Restock = false, List<CreditNoteLineRequest>? Lines = null);
}
