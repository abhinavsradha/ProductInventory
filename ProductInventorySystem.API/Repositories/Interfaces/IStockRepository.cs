using ProductInventorySystem.API.Models;

namespace ProductInventorySystem.API.Repositories.Interfaces;

public interface IStockRepository
{
    Task<decimal> GetSubVariantStockAsync(Guid subVariantId);
    Task UpdateSubVariantStockAsync(Guid subVariantId, decimal newStock);
    Task UpdateProductTotalStockAsync(Guid productId);
    Task AddTransactionAsync(StockTransaction transaction);
    Task<bool> SubVariantExistsAsync(Guid subVariantId, Guid productId);
}