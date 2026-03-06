namespace MiniERP.API.Security;

public static class AppRoles
{
    public const string OwnerAdmin = "Owner,Admin";
    public const string Sales = "Owner,Admin,Sales";
    public const string Purchasing = "Owner,Admin,Purchasing";
    public const string Inventory = "Owner,Admin,Inventory";
    public const string Accounting = "Owner,Admin,Accounting";
    public const string Catalogs = "Owner,Admin,Accounting,Sales,Purchasing";
    public const string Operations = "Owner,Admin,Sales,Purchasing,Inventory";
}
