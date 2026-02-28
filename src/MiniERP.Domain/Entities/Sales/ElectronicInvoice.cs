using System;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class ElectronicInvoice : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string DocumentType { get; set; } = "01";
    public string Key { get; set; } = string.Empty;
    public string Consecutive { get; set; } = string.Empty;
    public string? XmlUnsigned { get; set; }
    public string? XmlSigned { get; set; }
    public string Status { get; set; } = "Draft";
    public string? HaciendaStatus { get; set; }
    public string? HaciendaResponse { get; set; }
    public DateTime? SentAt { get; set; }
}
