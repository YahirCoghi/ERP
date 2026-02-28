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
public class ElectronicInvoicesController : ControllerBase
{
    private readonly IElectronicInvoiceService _electronicInvoiceService;

    public ElectronicInvoicesController(IElectronicInvoiceService electronicInvoiceService)
    {
        _electronicInvoiceService = electronicInvoiceService;
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue(IssueElectronicInvoiceRequest request)
    {
        try
        {
            var result = await _electronicInvoiceService.IssueAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> RefreshStatus(int id)
    {
        try
        {
            var result = await _electronicInvoiceService.RefreshStatusAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
