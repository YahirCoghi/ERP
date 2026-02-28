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
[Authorize(Roles = AppRoles.OwnerAdmin)]
public class CompanyProfilesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CompanyProfilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync();
        return Ok(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(CompanyProfile profile)
    {
        var existing = await _context.CompanyProfiles.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.CompanyProfiles.Add(profile);
        }
        else
        {
            existing.LegalName = profile.LegalName;
            existing.TradeName = profile.TradeName;
            existing.IdentificationType = profile.IdentificationType;
            existing.IdentificationNumber = profile.IdentificationNumber;
            existing.EconomicActivityCode = profile.EconomicActivityCode;
            existing.Email = profile.Email;
            existing.Phone = profile.Phone;
            existing.Address = profile.Address;
            existing.Province = profile.Province;
            existing.Canton = profile.Canton;
            existing.District = profile.District;
            existing.BranchCode = profile.BranchCode;
            existing.TerminalCode = profile.TerminalCode;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(profile);
    }
}
