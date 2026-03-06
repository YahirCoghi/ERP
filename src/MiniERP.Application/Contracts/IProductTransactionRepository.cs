using MiniERP.Domain.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniERP.Application.Contracts;

public interface IProductTransactionRepository
{
    Task<IEnumerable<ProductTransaction>> GetByProductAsync(int productId);
    Task AddAsync(ProductTransaction transaction);
}
