namespace ProductInventorySystem.API.Models;

public class StockTransaction
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SubVariantId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // PURCHASE or SALE
    public decimal Quantity { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Notes { get; set; }
}