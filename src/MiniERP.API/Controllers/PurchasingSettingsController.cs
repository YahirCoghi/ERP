using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.Purchasing;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.OwnerAdmin)]
public class PurchasingSettingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PurchasingSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _context.PurchasingSettings.FirstOrDefaultAsync();
        return Ok(settings ?? new PurchasingSettings());
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(PurchasingSettings settings)
    {
        var existing = await _context.PurchasingSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.PurchasingSettings.Add(settings);
        }
        else
        {
            existing.DefaultPaymentTermDays = settings.DefaultPaymentTermDays;
            existing.DefaultCurrency = settings.DefaultCurrency;
            existing.DefaultTaxRate = settings.DefaultTaxRate;
            existing.AllowPartialReceipt = settings.AllowPartialReceipt;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(settings);
    }
}
