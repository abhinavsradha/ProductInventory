using System.ComponentModel.DataAnnotations;

namespace ProductInventorySystem.API.DTOs;

public class StockRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid SubVariantId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public decimal Quantity { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}