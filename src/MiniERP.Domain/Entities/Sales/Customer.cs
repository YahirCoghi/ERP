using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }
    public IdentificationType? IdentificationType { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? EconomicActivityCode { get; set; }
}
