using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Models;

namespace ProductInventorySystem.API.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Guid> CreateProductAsync(Product product);
    Task<Guid> CreateVariantAsync(Variant variant);
    Task<Guid> CreateSubVariantAsync(SubVariant subVariant);
    Task<PagedResult<ProductListDto>> GetProductsAsync(int page, int pageSize, bool? active);
    Task<bool> ProductCodeExistsAsync(string productCode);
}