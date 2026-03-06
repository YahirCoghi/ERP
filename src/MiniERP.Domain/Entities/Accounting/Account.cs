using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Accounting;

public class Account : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public bool IsPosting { get; set; } = true;
}
