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
public class AccountsController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public AccountsController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var accounts = await _accountingService.GetAccountsAsync();
        return Ok(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        try
        {
            var account = await _accountingService.CreateAccountAsync(request);
            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
