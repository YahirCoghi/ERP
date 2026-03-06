using System;
using System.ComponentModel.DataAnnotations;

namespace MiniERP.Application.DTOs;

public class ProductCreateDto
{
    public string? Code { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Cost { get; set; }

    public int Stock { get; set; }

    public int MinStock { get; set; }

    public string? Category { get; set; }

    public string? Unit { get; set; }
}

public class ProductUpdateDto : ProductCreateDto
{
    [Required]
    public int Id { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
