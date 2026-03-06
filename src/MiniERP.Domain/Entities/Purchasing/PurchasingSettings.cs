using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Purchasing;

public class PurchasingSettings : BaseEntity
{
    public int DefaultPaymentTermDays { get; set; }
    public Currency DefaultCurrency { get; set; } = Currency.CRC;
    public decimal DefaultTaxRate { get; set; } = 13m;
    public bool AllowPartialReceipt { get; set; } = true;
}
