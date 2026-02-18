using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Models;
using ProductInventorySystem.API.Repositories.Interfaces;
using ProductInventorySystem.API.Services.Interfaces;

namespace ProductInventorySystem.API.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repo;
    private readonly ILogger<StockService> _logger;

    public StockService(IStockRepository repo, ILogger<StockService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task AddStockAsync(StockRequestDto dto)
    {
        if (!await _repo.SubVariantExistsAsync(dto.SubVariantId, dto.ProductId))
            throw new KeyNotFoundException("SubVariant not found for this product.");

        var current = await _repo.GetSubVariantStockAsync(dto.SubVariantId);
        await _repo.UpdateSubVariantStockAsync(dto.SubVariantId, current + dto.Quantity);
        await _repo.UpdateProductTotalStockAsync(dto.ProductId);
        await _repo.AddTransactionAsync(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            SubVariantId = dto.SubVariantId,
            TransactionType = "PURCHASE",
            Quantity = dto.Quantity,
            TransactionDate = DateTimeOffset.UtcNow,
            Notes = dto.Notes
        });
        _logger.LogInformation("Added {Qty} stock to SubVariant {Id}", dto.Quantity, dto.SubVariantId);
    }

    public async Task RemoveStockAsync(StockRequestDto dto)
    {
        if (!await _repo.SubVariantExistsAsync(dto.SubVariantId, dto.ProductId))
            throw new KeyNotFoundException("SubVariant not found for this product.");

        var current = await _repo.GetSubVariantStockAsync(dto.SubVariantId);
        if (current < dto.Quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {current}, Requested: {dto.Quantity}");

        await _repo.UpdateSubVariantStockAsync(dto.SubVariantId, current - dto.Quantity);
        await _repo.UpdateProductTotalStockAsync(dto.ProductId);
        await _repo.AddTransactionAsync(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = dto.ProductId,
            SubVariantId = dto.SubVariantId,
            TransactionType = "SALE",
            Quantity = dto.Quantity,
            TransactionDate = DateTimeOffset.UtcNow,
            Notes = dto.Notes
        });
        _logger.LogInformation("Removed {Qty} stock from SubVariant {Id}", dto.Quantity, dto.SubVariantId);
    }
}