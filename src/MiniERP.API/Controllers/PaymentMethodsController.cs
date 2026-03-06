using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.Common;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Catalogs)]
public class PaymentMethodsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentMethodsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _context.PaymentMethods.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(PaymentMethod item)
    {
        var existing = await _context.PaymentMethods.FirstOrDefaultAsync(t => t.Code == item.Code);
        if (existing == null)
        {
            _context.PaymentMethods.Add(item);
        }
        else
        {
            existing.Name = item.Name;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(item);
    }
}
