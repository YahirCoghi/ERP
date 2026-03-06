using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Controllers;
using MiniERP.Application.DTOs;
using MiniERP.Domain.Entities.Purchasing;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Tests;

public class AdvancedModulesControllerTests
{
    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Campaign_WizardSegment_CreatesRecipients()
    {
        await using var db = BuildDbContext();
        db.Customers.AddRange(
            new Customer { Code = "C1", Name = "Cliente 1", Email = "c1@test.com" },
            new Customer { Code = "C2", Name = "Cliente 2", Email = "c2@test.com" });
        await db.SaveChangesAsync();

        var campaignController = new CampaignsController(db);
        var createResult = await campaignController.Create(new CampaignDto(0, "Campana Q2", "{}", null, "Draft"));
        Assert.IsType<OkObjectResult>(createResult);
        var campaignId = db.Campaigns.Single().Id;

        var segmentResult = await campaignController.RunSegmentation(campaignId, 10);
        Assert.IsType<OkObjectResult>(segmentResult);
        Assert.Equal(2, db.CampaignRecipients.Count());
    }

    [Fact]
    public async Task FixedAsset_Depreciate_CreatesMonthlyEntry()
    {
        await using var db = BuildDbContext();
        var controller = new FinancialAdvancedController(db);
        var created = await controller.CreateFixedAsset(new FixedAssetDto(0, "FA-1", "Laptop", DateTime.UtcNow.Date, 1200m, 0m, 12, "Active"));
        Assert.IsType<OkObjectResult>(created);

        var assetId = db.FixedAssets.Single().Id;
        var depResult = await controller.Depreciate(assetId, DateTime.UtcNow.Date);
        Assert.IsType<OkObjectResult>(depResult);
        Assert.Single(db.FixedAssetDepreciations);
        Assert.Equal(100m, db.FixedAssetDepreciations.Single().Amount);
    }

    [Fact]
    public async Task Intrastat_Export_ReturnsCsv()
    {
        await using var db = BuildDbContext();
        var controller = new FinancialAdvancedController(db);
        var createResult = await controller.CreateIntrastat(new IntrastatDeclarationDto(0, "INT-001", 2026, 3, "Draft"));
        Assert.IsType<OkObjectResult>(createResult);
        var declarationId = db.IntrastatDeclarations.Single().Id;

        await controller.AddIntrastatLine(declarationId, new IntrastatDeclarationLineDto(0, declarationId, "1000", "DE", 10m, 250m));
        var export = await controller.ExportIntrastat(declarationId);
        var file = Assert.IsType<FileContentResult>(export);
        var csv = Encoding.UTF8.GetString(file.FileContents);
        Assert.Contains("INT-001,1000,DE,10", csv);
    }

    [Fact]
    public async Task Forms1099_Upsert_CreatesRecord()
    {
        await using var db = BuildDbContext();
        db.Suppliers.Add(new Supplier { Code = "SUP1", Name = "Supplier 1", Email = "sup@test.com" });
        await db.SaveChangesAsync();

        var supplierId = db.Suppliers.Single().Id;
        var controller = new FinancialAdvancedController(db);
        var upsert = await controller.Upsert1099(new Vendor1099AmountDto(0, supplierId, 2026, 5000m, 500m, "Test"));
        Assert.IsType<OkObjectResult>(upsert);
        Assert.Single(db.Vendor1099Amounts);
        Assert.Equal(5000m, db.Vendor1099Amounts.Single().NonEmployeeCompensation);
    }
}
