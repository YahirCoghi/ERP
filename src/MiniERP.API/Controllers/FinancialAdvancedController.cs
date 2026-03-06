using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Application.DTOs;
using MiniERP.Domain.Entities.Advanced;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Accounting)]
public class FinancialAdvancedController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public FinancialAdvancedController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("1099")]
    [Authorize(Policy = "Forms1099:Read")]
    public async Task<IActionResult> Get1099([FromQuery] int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var rows = await _db.Vendor1099Amounts
            .AsNoTracking()
            .Where(x => x.IsActive && x.Year == targetYear)
            .OrderBy(x => x.SupplierId)
            .Select(x => new Vendor1099AmountDto(x.Id, x.SupplierId, x.Year, x.NonEmployeeCompensation, x.FederalTaxWithheld, x.Notes))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("1099")]
    [Authorize(Policy = "Forms1099:Full")]
    public async Task<IActionResult> Upsert1099([FromBody] Vendor1099AmountDto request)
    {
        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == request.SupplierId && x.IsActive);
        if (!supplierExists)
            return BadRequest(new { message = "Supplier not found." });

        var row = await _db.Vendor1099Amounts.FirstOrDefaultAsync(x => x.SupplierId == request.SupplierId && x.Year == request.Year);
        if (row == null)
        {
            row = new Vendor1099Amount
            {
                SupplierId = request.SupplierId,
                Year = request.Year,
                NonEmployeeCompensation = request.NonEmployeeCompensation,
                FederalTaxWithheld = request.FederalTaxWithheld,
                Notes = request.Notes
            };
            _db.Vendor1099Amounts.Add(row);
        }
        else
        {
            row.NonEmployeeCompensation = request.NonEmployeeCompensation;
            row.FederalTaxWithheld = request.FederalTaxWithheld;
            row.Notes = request.Notes;
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new Vendor1099AmountDto(row.Id, row.SupplierId, row.Year, row.NonEmployeeCompensation, row.FederalTaxWithheld, row.Notes));
    }

    [HttpGet("fixed-assets")]
    [Authorize(Policy = "FixedAssets:Read")]
    public async Task<IActionResult> GetFixedAssets()
    {
        var rows = await _db.FixedAssets
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.AssetCode)
            .Select(x => new FixedAssetDto(x.Id, x.AssetCode, x.Name, x.AcquisitionDate, x.AcquisitionCost, x.ResidualValue, x.UsefulLifeMonths, x.Status))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("fixed-assets")]
    [Authorize(Policy = "FixedAssets:Full")]
    public async Task<IActionResult> CreateFixedAsset([FromBody] FixedAssetDto request)
    {
        var row = new FixedAsset
        {
            AssetCode = request.AssetCode.Trim(),
            Name = request.Name.Trim(),
            AcquisitionDate = request.AcquisitionDate,
            AcquisitionCost = request.AcquisitionCost,
            ResidualValue = request.ResidualValue,
            UsefulLifeMonths = request.UsefulLifeMonths,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim()
        };
        _db.FixedAssets.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new FixedAssetDto(row.Id, row.AssetCode, row.Name, row.AcquisitionDate, row.AcquisitionCost, row.ResidualValue, row.UsefulLifeMonths, row.Status));
    }

    [HttpPost("fixed-assets/{id:int}/depreciate")]
    [Authorize(Policy = "FixedAssets:Full")]
    public async Task<IActionResult> Depreciate(int id, [FromQuery] DateTime? period = null)
    {
        var asset = await _db.FixedAssets.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (asset == null)
            return NotFound(new { message = "Fixed asset not found." });
        if (!asset.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only active assets can be depreciated." });

        var monthly = (asset.AcquisitionCost - asset.ResidualValue) / Math.Max(asset.UsefulLifeMonths, 1);
        var row = new FixedAssetDepreciation
        {
            FixedAssetId = asset.Id,
            PeriodDate = (period ?? DateTime.UtcNow.Date),
            Amount = Math.Round(monthly, 2)
        };
        _db.FixedAssetDepreciations.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new FixedAssetDepreciationDto(row.Id, row.FixedAssetId, row.PeriodDate, row.Amount));
    }

    [HttpGet("intrastat")]
    [Authorize(Policy = "Intrastat:Read")]
    public async Task<IActionResult> GetIntrastat()
    {
        var rows = await _db.IntrastatDeclarations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Select(x => new IntrastatDeclarationDto(x.Id, x.DeclarationNumber, x.Year, x.Month, x.Status))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("intrastat")]
    [Authorize(Policy = "Intrastat:Full")]
    public async Task<IActionResult> CreateIntrastat([FromBody] IntrastatDeclarationDto request)
    {
        var row = new IntrastatDeclaration
        {
            DeclarationNumber = request.DeclarationNumber.Trim(),
            Year = request.Year,
            Month = request.Month,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim()
        };
        _db.IntrastatDeclarations.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new IntrastatDeclarationDto(row.Id, row.DeclarationNumber, row.Year, row.Month, row.Status));
    }

    [HttpPost("intrastat/{id:int}/lines")]
    [Authorize(Policy = "Intrastat:Full")]
    public async Task<IActionResult> AddIntrastatLine(int id, [FromBody] IntrastatDeclarationLineDto request)
    {
        var declarationExists = await _db.IntrastatDeclarations.AnyAsync(x => x.Id == id && x.IsActive);
        if (!declarationExists)
            return NotFound(new { message = "Declaration not found." });

        var row = new IntrastatDeclarationLine
        {
            IntrastatDeclarationId = id,
            CommodityCode = request.CommodityCode.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            NetMassKg = request.NetMassKg,
            ValueAmount = request.ValueAmount
        };
        _db.IntrastatDeclarationLines.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new IntrastatDeclarationLineDto(row.Id, row.IntrastatDeclarationId, row.CommodityCode, row.CountryCode, row.NetMassKg, row.ValueAmount));
    }

    [HttpGet("intrastat/{id:int}/export")]
    [Authorize(Policy = "Intrastat:Read")]
    public async Task<IActionResult> ExportIntrastat(int id)
    {
        var declaration = await _db.IntrastatDeclarations
            .AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (declaration == null)
            return NotFound(new { message = "Declaration not found." });

        var sb = new StringBuilder();
        sb.AppendLine("Declaration,CommodityCode,CountryCode,NetMassKg,ValueAmount");
        foreach (var line in declaration.Lines.Where(x => x.IsActive))
        {
            sb.AppendLine($"{declaration.DeclarationNumber},{line.CommodityCode},{line.CountryCode},{line.NetMassKg},{line.ValueAmount}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"intrastat_{declaration.DeclarationNumber}.csv");
    }
}
