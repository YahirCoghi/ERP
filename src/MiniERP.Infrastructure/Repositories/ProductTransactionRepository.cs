using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Inventory;
using MiniERP.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Repositories;

public class ProductTransactionRepository : IProductTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public ProductTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ProductTransaction transaction)
    {
        _context.ProductTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductTransaction>> GetByProductAsync(int productId)
    {
        return await _context.ProductTransactions.Where(t => t.ProductId == productId).OrderByDescending(t => t.Date).ToListAsync();
    }
}
