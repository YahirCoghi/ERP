using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public Currency Currency { get; set; } = Currency.CRC;
    public string Status { get; set; } = "Pending";
    public List<SalesOrderLine> Lines { get; set; } = new();
}
