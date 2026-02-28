using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Inventory;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public string? CabysCode { get; set; }
    public string? UnitMeasureCode { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsTaxExempt { get; set; }
}
