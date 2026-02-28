
using System;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Inventory;

public class ProductTransaction : BaseEntity
{
    // Type: Expense, Income
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
