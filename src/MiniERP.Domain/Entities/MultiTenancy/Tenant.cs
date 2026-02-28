using System;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.MultiTenancy;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? PaidUntil { get; set; }
    public int MaxUsers { get; set; } = 5;
    public string? PlanCode { get; set; }
    public string? BillingEmail { get; set; }
    public string? ExternalCustomerId { get; set; }
}
