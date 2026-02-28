namespace MiniERP.Domain.Entities.Common;

public class TaxCode : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsExempt { get; set; }
}
