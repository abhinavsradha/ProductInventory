using ProductInventorySystem.API.DTOs;

namespace ProductInventorySystem.API.Services.Interfaces;

public interface IProductService
{
    Task<Guid> CreateProductAsync(CreateProductDto dto);
    Task<PagedResult<ProductListDto>> GetProductsAsync(int page, int pageSize, bool? active);
}