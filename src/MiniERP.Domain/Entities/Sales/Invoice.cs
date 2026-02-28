using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public Currency Currency { get; set; } = Currency.CRC;
    public decimal ExchangeRate { get; set; } = 1m;
    public string SaleCondition { get; set; } = "01";
    public string PaymentMethod { get; set; } = "01";
    public string? ReferenceDocumentType { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? ReferenceCode { get; set; }
    public string? ReferenceReason { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? DueDate { get; set; }
    public List<InvoiceLine> Lines { get; set; } = new();
}
