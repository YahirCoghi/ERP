using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Purchasing;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public Currency Currency { get; set; } = Currency.CRC;
    public string Status { get; set; } = "Pending";
    public DateTime? ExpectedDate { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = new();
}
