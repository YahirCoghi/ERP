using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Domain.Entities.Inventory;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[Authorize(Roles = AppRoles.Inventory)]
[ApiController]
[Route("api/[controller]")]
public class InventoryMovementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventoryMovementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetMovements()
    {
        return await _context.InventoryMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetByProduct(int productId)
    {
        return await _context.InventoryMovements
            .Include(m => m.Product)
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<InventoryMovement>> CreateMovement(InventoryMovement movement)
    {
        var product = await _context.Products.FindAsync(movement.ProductId);
        if (product == null) return NotFound("Producto no encontrado");

        // Actualizar stock según tipo de movimiento
        switch (movement.Type)
        {
            case "IN":
                product.Stock += (int)movement.Quantity;
                break;
            case "OUT":
                if (product.Stock < movement.Quantity)
                    return BadRequest("Stock insuficiente");
                product.Stock -= (int)movement.Quantity;
                break;
            case "ADJUSTMENT":
                product.Stock = (int)movement.Quantity; // Ajuste directo
                break;
        }

        movement.TotalCost = movement.Quantity * movement.UnitCost;
        movement.CreatedAt = DateTime.UtcNow;

        _context.InventoryMovements.Add(movement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovements), new { id = movement.Id }, movement);
    }

    [HttpPost("receive-purchase/{purchaseOrderId}")]
    public async Task<ActionResult> ReceivePurchaseOrder(int purchaseOrderId)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

        if (purchaseOrder == null) return NotFound("Orden de compra no encontrada");

        foreach (var line in purchaseOrder.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product != null)
            {
                product.Stock += (int)line.Quantity;
                
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = line.ProductId,
                    Type = "IN",
                    Quantity = (int)line.Quantity,
                    UnitCost = line.UnitPrice,
                    TotalCost = line.Total,
                    Reference = purchaseOrder.OrderNumber,
                    ReferenceId = purchaseOrder.Id,
                    Notes = $"Recepción OC {purchaseOrder.OrderNumber}"
                });
            }
        }

        purchaseOrder.Status = "Received";
        purchaseOrder.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Recepción completada" });
    }
}
