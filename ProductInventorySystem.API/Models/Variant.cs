namespace ProductInventorySystem.API.Models;

public class Variant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; }
    public List<SubVariant> SubVariants { get; set; } = new();
}