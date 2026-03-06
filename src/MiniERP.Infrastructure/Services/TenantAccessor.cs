using MiniERP.Application.Contracts;

namespace MiniERP.Infrastructure.Services;

public class TenantAccessor : ITenantAccessor
{
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}
