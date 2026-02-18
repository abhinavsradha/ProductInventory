using ProductInventorySystem.API.DTOs;

namespace ProductInventorySystem.API.Services.Interfaces;

public interface IStockService
{
    Task AddStockAsync(StockRequestDto dto);
    Task RemoveStockAsync(StockRequestDto dto);
}