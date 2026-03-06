using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.API.Security;
using MiniERP.Application.DTOs;
using MiniERP.Application.Services;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Accounting)]
public class FiscalPeriodsController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public FiscalPeriodsController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var periods = await _accountingService.GetFiscalPeriodsAsync();
        return Ok(periods);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFiscalPeriodRequest request)
    {
        try
        {
            var period = await _accountingService.CreateFiscalPeriodAsync(request);
            return Ok(period);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        try
        {
            await _accountingService.CloseFiscalPeriodAsync(id);
            return Ok(new { message = "Fiscal period closed." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
