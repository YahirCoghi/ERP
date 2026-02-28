namespace MiniERP.Domain.Entities.Common;

public class CompanyProfile : BaseEntity
{
    public string LegalName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public IdentificationType IdentificationType { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;
    public string EconomicActivityCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Province { get; set; }
    public string? Canton { get; set; }
    public string? District { get; set; }
    public string BranchCode { get; set; } = "001";
    public string TerminalCode { get; set; } = "00001";
}
