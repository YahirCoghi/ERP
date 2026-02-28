using MiniERP.Application.DTOs;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Inventory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Code = dto.Code ?? string.Empty,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Cost = dto.Cost,
            Stock = dto.Stock,
            MinStock = dto.MinStock,
            Category = dto.Category,
            Unit = dto.Unit,
            CreatedAt = System.DateTime.UtcNow
        };

        if (string.IsNullOrWhiteSpace(product.Code))
        {
            var count = await _repo.CountAsync();
            product.Code = $"PRD-{(count + 1):D5}";
        }

        await _repo.AddAsync(product);

        return MapToDto(product);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) return;
        await _repo.DeleteAsync(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    public async Task UpdateAsync(ProductUpdateDto dto)
    {
        var product = await _repo.GetByIdAsync(dto.Id);
        if (product == null) return;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Cost = dto.Cost;
        product.Stock = dto.Stock;
        product.MinStock = dto.MinStock;
        product.Category = dto.Category;
        product.Unit = dto.Unit;
        product.UpdatedAt = System.DateTime.UtcNow;
        await _repo.UpdateAsync(product);
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Cost = p.Cost,
            Stock = p.Stock,
            MinStock = p.MinStock,
            Category = p.Category,
            Unit = p.Unit,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
