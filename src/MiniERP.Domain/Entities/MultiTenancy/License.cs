using System;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.MultiTenancy;

public class License : BaseEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime? ExpiresAt { get; set; }
    public int MaxUsers { get; set; } = 5;
}
