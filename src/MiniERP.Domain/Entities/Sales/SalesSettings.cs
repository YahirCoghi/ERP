using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class SalesSettings : BaseEntity
{
    public string DefaultSaleCondition { get; set; } = "01";
    public string DefaultPaymentMethod { get; set; } = "01";
    public int DefaultCreditDays { get; set; }
    public Currency DefaultCurrency { get; set; } = Currency.CRC;
    public decimal DefaultTaxRate { get; set; } = 13m;
    public bool AllowNegativeStock { get; set; }
}
