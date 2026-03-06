using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.MultiTenancy;

public class TenantUser : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Owner";
    public bool IsActive { get; set; } = true;
}
