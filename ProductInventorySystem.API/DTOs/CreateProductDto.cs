using System.ComponentModel.DataAnnotations;

namespace ProductInventorySystem.API.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "HSN Code is required.")]
    [MaxLength(100)]
    public string HSNCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "ProductCode is required.")]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    public Guid CreatedUser { get; set; }
    public bool IsFavourite { get; set; } = false;

    [Required(ErrorMessage = "At least one variant is required.")]
    [MinLength(1)]
    public List<VariantDto> Variants { get; set; } = new();
}

public class VariantDto
{
    [Required(ErrorMessage = "Variant name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Variant options are required.")]
    [MinLength(1)]
    public List<string> Options { get; set; } = new();
}