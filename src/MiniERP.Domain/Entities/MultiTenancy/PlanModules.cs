namespace MiniERP.Domain.Entities.MultiTenancy;

public static class PlanModules
{
    // Administration
    public const string CompanyAdmin = "CompanyAdmin";
    public const string ExchangeRates = "ExchangeRates";
    public const string Approvals = "Approvals";
    public const string Alerts = "Alerts";
    public const string DataUtilities = "DataUtilities";
    public const string Addons = "Addons";
    public const string Mobile = "Mobile";
    public const string WorkflowManager = "WorkflowManager";
    public const string LicenseManagement = "LicenseManagement";

    // Finance
    public const string Accounts = "Accounts";
    public const string JournalEntries = "JournalEntries";
    public const string Budgets = "Budgets";
    public const string InternalReconciliation = "InternalReconciliation";
    public const string FinancialReports = "FinancialReports";
    public const string FixedAssets = "FixedAssets";
    public const string Intrastat = "Intrastat";
    public const string Forms1099 = "Forms1099";

    // CRM / Sales
    public const string BusinessPartners = "BusinessPartners";
    public const string Activities = "Activities";
    public const string Opportunities = "Opportunities";
    public const string Quotations = "Quotations";
    public const string Campaigns = "Campaigns";
    public const string Customers = "Customers";
    public const string SalesOrders = "SalesOrders";

    // Purchasing / Logistics
    public const string Suppliers = "Suppliers";
    public const string PurchaseOrders = "PurchaseOrders";
    public const string PurchaseInvoices = "PurchaseInvoices";

    // Inventory
    public const string Products = "Products";
    public const string Bins = "Bins";
    public const string SerialsAndBarcodes = "SerialsAndBarcodes";
    public const string Inventory = "Inventory";
    public const string InventoryTransfers = "InventoryTransfers";
    public const string PriceLists = "PriceLists";
    public const string InventoryCounts = "InventoryCounts";

    // Production / MRP
    public const string Resources = "Resources";
    public const string Bom = "Bom";
    public const string ProductionOrders = "ProductionOrders";
    public const string Mrp = "Mrp";
    public const string ProcurementWizard = "ProcurementWizard";
    public const string PickPack = "PickPack";

    // Service / HR / Projects / Tools
    public const string Service = "Service";
    public const string KnowledgeBase = "KnowledgeBase";
    public const string ServiceReports = "ServiceReports";
    public const string Hr = "Hr";
    public const string Projects = "Projects";
    public const string QueryManager = "QueryManager";
    public const string PrintLayouts = "PrintLayouts";
    public const string UserDefinedObjects = "UserDefinedObjects";

    // General
    public const string Accounting = "Accounting";
    public const string Reports = "Reports";
}
