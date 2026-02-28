using MiniERP.Domain.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniERP.Application.Contracts;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<int> CountAsync();
}
