using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class PlanService : IPlanService
{
    private static readonly string[] AllModules =
    [
        PlanModules.CompanyAdmin,
        PlanModules.ExchangeRates,
        PlanModules.Approvals,
        PlanModules.Alerts,
        PlanModules.DataUtilities,
        PlanModules.Addons,
        PlanModules.Mobile,
        PlanModules.WorkflowManager,
        PlanModules.LicenseManagement,
        PlanModules.Accounts,
        PlanModules.JournalEntries,
        PlanModules.Budgets,
        PlanModules.InternalReconciliation,
        PlanModules.FinancialReports,
        PlanModules.FixedAssets,
        PlanModules.Intrastat,
        PlanModules.Forms1099,
        PlanModules.BusinessPartners,
        PlanModules.Activities,
        PlanModules.Opportunities,
        PlanModules.Quotations,
        PlanModules.Campaigns,
        PlanModules.Customers,
        PlanModules.SalesOrders,
        PlanModules.Suppliers,
        PlanModules.PurchaseOrders,
        PlanModules.PurchaseInvoices,
        PlanModules.Products,
        PlanModules.Bins,
        PlanModules.SerialsAndBarcodes,
        PlanModules.Inventory,
        PlanModules.InventoryTransfers,
        PlanModules.PriceLists,
        PlanModules.InventoryCounts,
        PlanModules.Resources,
        PlanModules.Bom,
        PlanModules.ProductionOrders,
        PlanModules.Mrp,
        PlanModules.ProcurementWizard,
        PlanModules.PickPack,
        PlanModules.Service,
        PlanModules.KnowledgeBase,
        PlanModules.ServiceReports,
        PlanModules.Hr,
        PlanModules.Projects,
        PlanModules.QueryManager,
        PlanModules.PrintLayouts,
        PlanModules.UserDefinedObjects,
        PlanModules.Accounting,
        PlanModules.Reports
    ];

    private static readonly IReadOnlyDictionary<PlanType, IReadOnlyDictionary<string, ModuleAccessLevel>> DefaultDefinitions =
        new Dictionary<PlanType, IReadOnlyDictionary<string, ModuleAccessLevel>>
        {
            [PlanType.Starter] = BuildWithOverrides(ModuleAccessLevel.None, new Dictionary<string, ModuleAccessLevel>
            {
                [PlanModules.BusinessPartners] = ModuleAccessLevel.Read,
                [PlanModules.Customers] = ModuleAccessLevel.Full,
                [PlanModules.Products] = ModuleAccessLevel.Read,
                [PlanModules.SalesOrders] = ModuleAccessLevel.Full,
                [PlanModules.Quotations] = ModuleAccessLevel.Read,
                [PlanModules.Inventory] = ModuleAccessLevel.Read,
                [PlanModules.Reports] = ModuleAccessLevel.Read,
                [PlanModules.Alerts] = ModuleAccessLevel.Read,
                [PlanModules.KnowledgeBase] = ModuleAccessLevel.Read,
                [PlanModules.Campaigns] = ModuleAccessLevel.Read
            }),
            [PlanType.Profesional] = BuildWithOverrides(ModuleAccessLevel.Full, new Dictionary<string, ModuleAccessLevel>()),
            [PlanType.LimitadoVentas] = BuildWithOverrides(ModuleAccessLevel.None, new Dictionary<string, ModuleAccessLevel>
            {
                [PlanModules.BusinessPartners] = ModuleAccessLevel.Full,
                [PlanModules.Customers] = ModuleAccessLevel.Full,
                [PlanModules.Products] = ModuleAccessLevel.Read,
                [PlanModules.Activities] = ModuleAccessLevel.Full,
                [PlanModules.Opportunities] = ModuleAccessLevel.Full,
                [PlanModules.Quotations] = ModuleAccessLevel.Full,
                [PlanModules.SalesOrders] = ModuleAccessLevel.Full,
                [PlanModules.Reports] = ModuleAccessLevel.Read,
                [PlanModules.Alerts] = ModuleAccessLevel.Read,
                [PlanModules.Campaigns] = ModuleAccessLevel.Full,
                [PlanModules.KnowledgeBase] = ModuleAccessLevel.Read
            }),
            [PlanType.LimitadoInventario] = BuildWithOverrides(ModuleAccessLevel.None, new Dictionary<string, ModuleAccessLevel>
            {
                [PlanModules.Products] = ModuleAccessLevel.Full,
                [PlanModules.Bins] = ModuleAccessLevel.Full,
                [PlanModules.SerialsAndBarcodes] = ModuleAccessLevel.Full,
                [PlanModules.Inventory] = ModuleAccessLevel.Full,
                [PlanModules.InventoryTransfers] = ModuleAccessLevel.Full,
                [PlanModules.InventoryCounts] = ModuleAccessLevel.Full,
                [PlanModules.PriceLists] = ModuleAccessLevel.Read,
                [PlanModules.PurchaseOrders] = ModuleAccessLevel.Full,
                [PlanModules.PurchaseInvoices] = ModuleAccessLevel.Read,
                [PlanModules.Reports] = ModuleAccessLevel.Read,
                [PlanModules.PickPack] = ModuleAccessLevel.Full,
                [PlanModules.ProcurementWizard] = ModuleAccessLevel.Read
            }),
            [PlanType.LimitadoFinanzas] = BuildWithOverrides(ModuleAccessLevel.None, new Dictionary<string, ModuleAccessLevel>
            {
                [PlanModules.Accounting] = ModuleAccessLevel.Full,
                [PlanModules.Accounts] = ModuleAccessLevel.Full,
                [PlanModules.JournalEntries] = ModuleAccessLevel.Full,
                [PlanModules.Budgets] = ModuleAccessLevel.Full,
                [PlanModules.InternalReconciliation] = ModuleAccessLevel.Full,
                [PlanModules.ExchangeRates] = ModuleAccessLevel.Full,
                [PlanModules.FinancialReports] = ModuleAccessLevel.Full,
                [PlanModules.Reports] = ModuleAccessLevel.Full,
                [PlanModules.Alerts] = ModuleAccessLevel.Read,
                [PlanModules.FixedAssets] = ModuleAccessLevel.Full,
                [PlanModules.Intrastat] = ModuleAccessLevel.Full,
                [PlanModules.Forms1099] = ModuleAccessLevel.Full
            })
        };

    private readonly MasterDbContext _masterDb;

    public PlanService(MasterDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _masterDb.PlanFeatures
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var changed = false;

        foreach (var (planType, modules) in DefaultDefinitions)
        {
            foreach (var (module, access) in modules)
            {
                var row = existing.FirstOrDefault(x =>
                    x.PlanType == planType &&
                    x.Module.Equals(module, StringComparison.OrdinalIgnoreCase));

                if (row == null)
                {
                    _masterDb.PlanFeatures.Add(new PlanFeature
                    {
                        PlanType = planType,
                        Module = module,
                        AccessLevel = access
                    });
                    changed = true;
                    continue;
                }

                if (row.AccessLevel != access)
                {
                    var tracked = await _masterDb.PlanFeatures.FirstAsync(x => x.Id == row.Id, cancellationToken);
                    tracked.AccessLevel = access;
                    tracked.UpdatedAt = DateTime.UtcNow;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            await _masterDb.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ModuleAccessLevel> GetAccessLevelAsync(int tenantId, string module, CancellationToken cancellationToken = default)
    {
        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);
        if (tenant == null)
            return ModuleAccessLevel.None;

        var normalized = NormalizeModule(module);
        var feature = await _masterDb.PlanFeatures
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.PlanType == tenant.PlanType && f.Module == normalized, cancellationToken);

        return feature?.AccessLevel ?? ModuleAccessLevel.None;
    }

    public async Task<bool> HasAccessAsync(int tenantId, string module, ModuleAccessLevel required, CancellationToken cancellationToken = default)
    {
        var level = await GetAccessLevelAsync(tenantId, module, cancellationToken);
        return level >= required;
    }

    public async Task<IReadOnlyList<PlanModuleAccess>> GetPlanModulesForTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive, cancellationToken);
        if (tenant == null)
            return Array.Empty<PlanModuleAccess>();

        var modules = await _masterDb.PlanFeatures
            .AsNoTracking()
            .Where(f => f.PlanType == tenant.PlanType)
            .OrderBy(f => f.Module)
            .Select(f => new PlanModuleAccess(f.Module, f.AccessLevel))
            .ToListAsync(cancellationToken);

        return modules;
    }

    private static string NormalizeModule(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
            return string.Empty;

        return module.Trim();
    }

    private static IReadOnlyDictionary<string, ModuleAccessLevel> BuildWithOverrides(
        ModuleAccessLevel baseline,
        Dictionary<string, ModuleAccessLevel> overrides)
    {
        var map = new Dictionary<string, ModuleAccessLevel>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in AllModules)
        {
            map[module] = baseline;
        }

        foreach (var (module, level) in overrides)
        {
            map[module] = level;
        }

        return map;
    }
}
