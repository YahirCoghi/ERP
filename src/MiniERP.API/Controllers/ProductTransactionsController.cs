using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniERP.API.Security;
using MiniERP.Application.Services;
using MiniERP.Application.DTOs;
using System.Threading.Tasks;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/[controller]")]
[Authorize(Roles = AppRoles.Inventory)]
public class TransactionsController : ControllerBase
{
    private readonly IProductTransactionService _service;

    public TransactionsController(IProductTransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var list = await _service.GetByProductAsync(productId);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, [FromBody] ProductTransactionCreateDto dto)
    {
        dto.ProductId = productId;
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByProduct), new { productId = productId }, created);
    }
}
