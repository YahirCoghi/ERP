using MiniERP.Application.Contracts;
using MiniERP.Application.DTOs;
using MiniERP.Domain.Entities.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniERP.Application.Services;

public class ProductTransactionService : IProductTransactionService
{
    private readonly IProductTransactionRepository _repo;

    public ProductTransactionService(IProductTransactionRepository repo)
    {
        _repo = repo;
    }

    public async Task<ProductTransactionDto> CreateAsync(ProductTransactionCreateDto dto)
    {
        var t = new ProductTransaction
        {
            Type = dto.Type,
            Amount = dto.Amount,
            Description = dto.Description,
            Date = dto.Date ?? System.DateTime.UtcNow,
            ProductId = dto.ProductId
        };

        await _repo.AddAsync(t);

        return new ProductTransactionDto
        {
            Id = t.Id,
            Type = t.Type,
            Amount = t.Amount,
            Description = t.Description,
            Date = t.Date,
            ProductId = t.ProductId
        };
    }

    public async Task<IEnumerable<ProductTransactionDto>> GetByProductAsync(int productId)
    {
        var list = await _repo.GetByProductAsync(productId);
        return list.Select(t => new ProductTransactionDto
        {
            Id = t.Id,
            Type = t.Type,
            Amount = t.Amount,
            Description = t.Description,
            Date = t.Date,
            ProductId = t.ProductId
        });
    }
}
