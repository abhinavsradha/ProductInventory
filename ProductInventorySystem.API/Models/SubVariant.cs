namespace ProductInventorySystem.API.Models;

public class SubVariant
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string OptionValue { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string? SKU { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}