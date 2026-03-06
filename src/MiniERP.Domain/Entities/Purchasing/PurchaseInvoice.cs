using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Purchasing;

public class PurchaseInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string Status { get; set; } = "Issued";
    public DateTime? DueDate { get; set; }
    public List<PurchaseInvoiceLine> Lines { get; set; } = new();
}
