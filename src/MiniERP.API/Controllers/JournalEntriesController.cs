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
public class JournalEntriesController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public JournalEntriesController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var entries = await _accountingService.GetJournalEntriesAsync();
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJournalEntryRequest request)
    {
        try
        {
            var entry = await _accountingService.CreateJournalEntryAsync(request);
            return Ok(entry);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        try
        {
            var entry = await _accountingService.PostJournalEntryAsync(id);
            return Ok(entry);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
