using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Models;
using ProductInventorySystem.API.Repositories.Interfaces;
using ProductInventorySystem.API.Services.Interfaces;

namespace ProductInventorySystem.API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repo, ILogger<ProductService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Guid> CreateProductAsync(CreateProductDto dto)
    {
        if (await _repo.ProductCodeExistsAsync(dto.ProductCode))
            throw new InvalidOperationException($"Product code '{dto.ProductCode}' already exists.");

        var now = DateTimeOffset.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            ProductCode = dto.ProductCode,
            ProductName = dto.Name,
            HSNCode = dto.HSNCode,
            CreatedUser = dto.CreatedUser == Guid.Empty ? Guid.NewGuid() : dto.CreatedUser,
            IsFavourite = dto.IsFavourite,
            Active = true,
            TotalStock = 0,
            CreatedDate = now,
            UpdatedDate = now
        };

        await _repo.CreateProductAsync(product);

        foreach (var variantDto in dto.Variants)
        {
            var variant = new Variant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Name = variantDto.Name,
                CreatedDate = now
            };
            await _repo.CreateVariantAsync(variant);

            foreach (var option in variantDto.Options)
            {
                var subVariant = new SubVariant
                {
                    Id = Guid.NewGuid(),
                    VariantId = variant.Id,
                    ProductId = product.Id,
                    OptionValue = option,
                    Stock = 0,
                    SKU = $"{dto.ProductCode}-{variantDto.Name}-{option}".ToUpper(),
                    CreatedDate = now
                };
                await _repo.CreateSubVariantAsync(subVariant);
            }
        }

        _logger.LogInformation("Product {Name} created with ID {Id}", dto.Name, product.Id);
        return product.Id;
    }

    public Task<PagedResult<ProductListDto>> GetProductsAsync(int page, int pageSize, bool? active)
        => _repo.GetProductsAsync(page, pageSize, active);
}