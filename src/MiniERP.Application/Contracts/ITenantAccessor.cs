namespace MiniERP.Application.Contracts;

public interface ITenantAccessor
{
    int? TenantId { get; set; }
    string? TenantName { get; set; }
    string ConnectionString { get; set; }
}
