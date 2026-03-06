using MiniERP.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniERP.Application.Services;

public interface IProductTransactionService
{
    Task<IEnumerable<ProductTransactionDto>> GetByProductAsync(int productId);
    Task<ProductTransactionDto> CreateAsync(ProductTransactionCreateDto dto);
}
