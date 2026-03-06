using System;
using System.ComponentModel.DataAnnotations;

namespace MiniERP.Application.DTOs;

public class ProductTransactionCreateDto
{
    [Required]
    public string Type { get; set; } = string.Empty; // "Expense" or "Income"

    [Required]
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    [Required]
    public int ProductId { get; set; }
}

public class ProductTransactionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public int ProductId { get; set; }
}
