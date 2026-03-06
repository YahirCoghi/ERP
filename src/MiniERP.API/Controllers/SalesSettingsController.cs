using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.OwnerAdmin)]
public class SalesSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SalesSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _context.SalesSettings.FirstOrDefaultAsync();
        return Ok(settings ?? new SalesSettings());
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(SalesSettings settings)
    {
        var existing = await _context.SalesSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.SalesSettings.Add(settings);
        }
        else
        {
            existing.DefaultSaleCondition = settings.DefaultSaleCondition;
            existing.DefaultPaymentMethod = settings.DefaultPaymentMethod;
            existing.DefaultCreditDays = settings.DefaultCreditDays;
            existing.DefaultCurrency = settings.DefaultCurrency;
            existing.DefaultTaxRate = settings.DefaultTaxRate;
            existing.AllowNegativeStock = settings.AllowNegativeStock;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(settings);
    }
}
