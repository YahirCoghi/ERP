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
public class TaxCodesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TaxCodesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _context.TaxCodes.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(TaxCode item)
    {
        var existing = await _context.TaxCodes.FirstOrDefaultAsync(t => t.Code == item.Code);
        if (existing == null)
        {
            _context.TaxCodes.Add(item);
        }
        else
        {
            existing.Name = item.Name;
            existing.Rate = item.Rate;
            existing.IsExempt = item.IsExempt;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(item);
    }
}
