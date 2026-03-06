using MiniERP.Domain.Entities.Common;
using MiniERP.Domain.Entities.Inventory;

namespace MiniERP.Domain.Entities.Purchasing;

public class PurchaseInvoiceLine : BaseEntity
{
    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}
