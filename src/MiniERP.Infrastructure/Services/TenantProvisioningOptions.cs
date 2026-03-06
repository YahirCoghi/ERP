namespace MiniERP.Infrastructure.Services;

public class TenantProvisioningOptions
{
    public string AdminConnectionString { get; set; } = string.Empty;
    public string TenantConnectionTemplate { get; set; } = string.Empty;
}
