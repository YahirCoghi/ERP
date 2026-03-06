using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities.Accounting;
using MiniERP.Domain.Entities.Advanced;
using MiniERP.Domain.Entities.Common;
using MiniERP.Domain.Entities.Inventory;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Domain.Entities.Purchasing;
using MiniERP.Domain.Entities.Auth;

namespace MiniERP.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<SalesSettings> SalesSettings => Set<SalesSettings>();
    public DbSet<PurchasingSettings> PurchasingSettings => Set<PurchasingSettings>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();
    public DbSet<User> Users => Set<User>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<ProductTransaction> ProductTransactions => Set<ProductTransaction>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<RecurringJournalTemplate> RecurringJournalTemplates => Set<RecurringJournalTemplate>();
    public DbSet<RecurringJournalTemplateLine> RecurringJournalTemplateLines => Set<RecurringJournalTemplateLine>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<SystemAlert> SystemAlerts => Set<SystemAlert>();
    public DbSet<ElectronicInvoice> ElectronicInvoices => Set<ElectronicInvoice>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<TaxCode> TaxCodes => Set<TaxCode>();
    public DbSet<EntitySequence> EntitySequences => Set<EntitySequence>();
    public DbSet<SaleCondition> SaleConditions => Set<SaleCondition>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentTerm> PaymentTerms => Set<PaymentTerm>();
    public DbSet<ApprovalTemplate> ApprovalTemplates => Set<ApprovalTemplate>();
    public DbSet<ApprovalTemplateStage> ApprovalTemplateStages => Set<ApprovalTemplateStage>();
    public DbSet<AddonDefinition> AddonDefinitions => Set<AddonDefinition>();
    public DbSet<AddonActivation> AddonActivations => Set<AddonActivation>();
    public DbSet<MobileServiceConfig> MobileServiceConfigs => Set<MobileServiceConfig>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<TenantLicenseAssignment> TenantLicenseAssignments => Set<TenantLicenseAssignment>();
    public DbSet<Vendor1099Amount> Vendor1099Amounts => Set<Vendor1099Amount>();
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();
    public DbSet<FixedAssetDepreciation> FixedAssetDepreciations => Set<FixedAssetDepreciation>();
    public DbSet<IntrastatDeclaration> IntrastatDeclarations => Set<IntrastatDeclaration>();
    public DbSet<IntrastatDeclarationLine> IntrastatDeclarationLines => Set<IntrastatDeclarationLine>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRecipient> CampaignRecipients => Set<CampaignRecipient>();
    public DbSet<ProcurementConfirmation> ProcurementConfirmations => Set<ProcurementConfirmation>();
    public DbSet<ProcurementConfirmationLine> ProcurementConfirmationLines => Set<ProcurementConfirmationLine>();
    public DbSet<PickPackTask> PickPackTasks => Set<PickPackTask>();
    public DbSet<PickPackTaskLine> PickPackTaskLines => Set<PickPackTaskLine>();
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles => Set<KnowledgeBaseArticle>();
    public DbSet<ServiceSlaMetric> ServiceSlaMetrics => Set<ServiceSlaMetric>();
    public DbSet<QueryDefinition> QueryDefinitions => Set<QueryDefinition>();
    public DbSet<PrintLayoutTemplate> PrintLayoutTemplates => Set<PrintLayoutTemplate>();
    public DbSet<UserDefinedField> UserDefinedFields => Set<UserDefinedField>();
    public DbSet<UserDefinedObject> UserDefinedObjects => Set<UserDefinedObject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CabysCode).HasMaxLength(20);
            entity.Property(e => e.UnitMeasureCode).HasMaxLength(10);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IdentificationNumber).HasMaxLength(20);
            entity.Property(e => e.EconomicActivityCode).HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IdentificationNumber).HasMaxLength(20);
            entity.Property(e => e.EconomicActivityCode).HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<ProductTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
        });

        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.SalesOrder).WithMany(e => e.Lines).HasForeignKey(e => e.SalesOrderId);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId);
            entity.HasOne(e => e.SalesOrder).WithMany().HasForeignKey(e => e.SalesOrderId);
            entity.Property(e => e.SaleCondition).HasMaxLength(2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(2);
            entity.Property(e => e.ReferenceDocumentType).HasMaxLength(2);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            entity.Property(e => e.ReferenceCode).HasMaxLength(2);
            entity.Property(e => e.ReferenceReason).HasMaxLength(180);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.Invoice).WithMany(e => e.Lines).HasForeignKey(e => e.InvoiceId);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
        });

        modelBuilder.Entity<EntitySequence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Prefix).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Padding).HasDefaultValue(6);
            entity.HasIndex(e => e.EntityName).IsUnique();
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.PurchaseOrder).WithMany(e => e.Lines).HasForeignKey(e => e.PurchaseOrderId);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierId);
            entity.HasOne(e => e.PurchaseOrder).WithMany().HasForeignKey(e => e.PurchaseOrderId);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
        });

        modelBuilder.Entity<PurchaseInvoiceLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.PurchaseInvoice).WithMany(e => e.Lines).HasForeignKey(e => e.PurchaseInvoiceId);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<SalesSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultSaleCondition).HasMaxLength(2);
            entity.Property(e => e.DefaultPaymentMethod).HasMaxLength(2);
            entity.Property(e => e.DefaultTaxRate).HasColumnType("decimal(5,2)");
        });

        modelBuilder.Entity<PurchasingSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DefaultTaxRate).HasColumnType("decimal(5,2)");
        });

        modelBuilder.Entity<TaxCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Rate).HasColumnType("decimal(5,2)");
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<SaleCondition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(2);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(2);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<PaymentTerm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<ApprovalTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(60);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<ApprovalTemplateStage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.RoleRequired).IsRequired().HasMaxLength(60);
            entity.Property(e => e.MinAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Template).WithMany(t => t.Stages).HasForeignKey(e => e.TemplateId);
            entity.HasIndex(e => new { e.TemplateId, e.StageOrder }).IsUnique();
        });

        modelBuilder.Entity<AddonDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(40);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<AddonActivation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.AddonDefinition).WithMany().HasForeignKey(e => e.AddonDefinitionId);
            entity.HasIndex(e => e.AddonDefinitionId).IsUnique();
        });

        modelBuilder.Entity<MobileServiceConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(80);
        });

        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(60);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<WorkflowStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.ActionType).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.WorkflowDefinition).WithMany(w => w.Steps).HasForeignKey(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => new { e.WorkflowDefinitionId, e.StepOrder }).IsUnique();
        });

        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceType).IsRequired().HasMaxLength(60);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.HasOne(e => e.WorkflowDefinition).WithMany().HasForeignKey(e => e.WorkflowDefinitionId);
            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId });
        });

        modelBuilder.Entity<TenantLicenseAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicenseCode).IsRequired().HasMaxLength(60);
            entity.Property(e => e.Scope).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.LicenseCode, e.UserId });
        });

        modelBuilder.Entity<Vendor1099Amount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NonEmployeeCompensation).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FederalTaxWithheld).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.SupplierId, e.Year }).IsUnique();
        });

        modelBuilder.Entity<FixedAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssetCode).IsRequired().HasMaxLength(40);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AcquisitionCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ResidualValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.AssetCode).IsUnique();
        });

        modelBuilder.Entity<FixedAssetDepreciation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.FixedAsset).WithMany().HasForeignKey(e => e.FixedAssetId);
            entity.HasIndex(e => new { e.FixedAssetId, e.PeriodDate }).IsUnique();
        });

        modelBuilder.Entity<IntrastatDeclaration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeclarationNumber).IsRequired().HasMaxLength(40);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.DeclarationNumber).IsUnique();
        });

        modelBuilder.Entity<IntrastatDeclarationLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommodityCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(3);
            entity.Property(e => e.NetMassKg).HasColumnType("decimal(18,3)");
            entity.Property(e => e.ValueAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.IntrastatDeclaration).WithMany(d => d.Lines).HasForeignKey(e => e.IntrastatDeclarationId);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<CampaignRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Campaign).WithMany(c => c.Recipients).HasForeignKey(e => e.CampaignId);
            entity.HasIndex(e => new { e.CampaignId, e.CustomerId }).IsUnique();
        });

        modelBuilder.Entity<ProcurementConfirmation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceType).IsRequired().HasMaxLength(40);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<ProcurementConfirmationLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestedQty).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ConfirmedQty).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.ProcurementConfirmation).WithMany(p => p.Lines).HasForeignKey(e => e.ProcurementConfirmationId);
        });

        modelBuilder.Entity<PickPackTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskNumber).IsRequired().HasMaxLength(40);
            entity.Property(e => e.TaskType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Warehouse).HasMaxLength(50);
            entity.HasIndex(e => e.TaskNumber).IsUnique();
        });

        modelBuilder.Entity<PickPackTaskLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ConfirmedQuantity).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.PickPackTask).WithMany(t => t.Lines).HasForeignKey(e => e.PickPackTaskId);
        });

        modelBuilder.Entity<KnowledgeBaseArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(180);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Tags).HasMaxLength(200);
        });

        modelBuilder.Entity<ServiceSlaMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AvgResolutionHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SlaCompliancePct).HasColumnType("decimal(5,2)");
            entity.HasIndex(e => e.MetricDate);
        });

        modelBuilder.Entity<QueryDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.SqlText).IsRequired();
            entity.Property(e => e.Module).IsRequired().HasMaxLength(60);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<PrintLayoutTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(60);
            entity.Property(e => e.LayoutJson).IsRequired();
        });

        modelBuilder.Entity<UserDefinedField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetEntity).IsRequired().HasMaxLength(120);
            entity.Property(e => e.FieldName).IsRequired().HasMaxLength(120);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => new { e.TargetEntity, e.FieldName }).IsUnique();
        });

        modelBuilder.Entity<UserDefinedObject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ObjectName).IsRequired().HasMaxLength(120);
            entity.Property(e => e.SchemaJson).IsRequired();
            entity.HasIndex(e => e.ObjectName).IsUnique();
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.ParentAccount).WithMany().HasForeignKey(e => e.ParentAccountId);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromCurrency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.ToCurrency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Rate).HasColumnType("decimal(18,6)");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.HasIndex(e => new { e.RateDate, e.FromCurrency, e.ToCurrency }).IsUnique();
        });

        modelBuilder.Entity<RecurringJournalTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Frequency).HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<RecurringJournalTemplateLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Debit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Credit).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Template).WithMany(t => t.Lines).HasForeignKey(e => e.TemplateId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<ApprovalRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Module).IsRequired().HasMaxLength(60);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(60);
            entity.Property(e => e.RequestedBy).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DecisionBy).HasMaxLength(120);
        });

        modelBuilder.Entity<SystemAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(160);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(800);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntryNumber).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.FiscalPeriod).WithMany().HasForeignKey(e => e.FiscalPeriodId);
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.JournalEntry).WithMany(e => e.Lines).HasForeignKey(e => e.JournalEntryId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
            entity.Property(e => e.Debit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Credit).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ElectronicInvoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId);
            entity.Property(e => e.DocumentType).HasMaxLength(2);
            entity.Property(e => e.Key).HasMaxLength(50);
            entity.Property(e => e.Consecutive).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.HaciendaStatus).HasMaxLength(30);
        });

        modelBuilder.Entity<DocumentSequence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).HasMaxLength(2);
            entity.Property(e => e.BranchCode).HasMaxLength(3);
            entity.Property(e => e.TerminalCode).HasMaxLength(5);
            entity.HasIndex(e => new { e.DocumentType, e.BranchCode, e.TerminalCode }).IsUnique();
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LegalName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IdentificationNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EconomicActivityCode).HasMaxLength(20);
        });
    }
}
