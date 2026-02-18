namespace ProductInventorySystem.API.DTOs;

public class ProductListDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public bool IsFavourite { get; set; }
    public bool Active { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal TotalStock { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public List<VariantListDto> Variants { get; set; } = new();
}

public class VariantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SubVariantListDto> SubVariants { get; set; } = new();
}

public class SubVariantListDto
{
    public Guid Id { get; set; }
    public string OptionValue { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string? SKU { get; set; }
}