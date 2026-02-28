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
public class SaleConditionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SaleConditionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _context.SaleConditions.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(SaleCondition item)
    {
        var existing = await _context.SaleConditions.FirstOrDefaultAsync(t => t.Code == item.Code);
        if (existing == null)
        {
            _context.SaleConditions.Add(item);
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
