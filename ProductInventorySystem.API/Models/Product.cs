namespace ProductInventorySystem.API.Models;

public class Product
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public byte[]? ProductImage { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public Guid CreatedUser { get; set; }
    public bool IsFavourite { get; set; }
    public bool Active { get; set; }
    public string HSNCode { get; set; } = string.Empty;
    public decimal TotalStock { get; set; }
    public List<Variant> Variants { get; set; } = new();
}