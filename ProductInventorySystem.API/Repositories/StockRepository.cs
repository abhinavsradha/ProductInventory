using Microsoft.Data.SqlClient;
using ProductInventorySystem.API.Data;
using ProductInventorySystem.API.Models;
using ProductInventorySystem.API.Repositories.Interfaces;

namespace ProductInventorySystem.API.Repositories;

public class StockRepository : IStockRepository
{
    private readonly DbConnectionFactory _factory;
    private readonly ILogger<StockRepository> _logger;

    public StockRepository(DbConnectionFactory factory, ILogger<StockRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<bool> SubVariantExistsAsync(Guid subVariantId, Guid productId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM SubVariants WHERE Id=@Id AND ProductId=@ProductId", conn);
        cmd.Parameters.AddWithValue("@Id", subVariantId);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task<decimal> GetSubVariantStockAsync(Guid subVariantId)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT Stock FROM SubVariants WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", subVariantId);
        var result = await cmd.ExecuteScalarAsync();
        return result == null ? 0 : (decimal)result;
    }

    public async Task UpdateSubVariantStockAsync(Guid subVariantId, decimal newStock)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE SubVariants SET Stock = @Stock WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Stock", newStock);
        cmd.Parameters.AddWithValue("@Id", subVariantId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateProductTotalStockAsync(Guid productId)
    {
        // Recalculate total from all subvariants
        const string sql = @"
            UPDATE Products 
            SET TotalStock = (SELECT ISNULL(SUM(Stock), 0) FROM SubVariants WHERE ProductId = @ProductId),
                UpdatedDate = SYSDATETIMEOFFSET()
            WHERE Id = @ProductId";
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task AddTransactionAsync(StockTransaction transaction)
    {
        const string sql = @"
            INSERT INTO StockTransactions (Id, ProductId, SubVariantId, TransactionType, Quantity, TransactionDate, Notes)
            VALUES (@Id, @ProductId, @SubVariantId, @TransactionType, @Quantity, @TransactionDate, @Notes)";
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", transaction.Id);
        cmd.Parameters.AddWithValue("@ProductId", transaction.ProductId);
        cmd.Parameters.AddWithValue("@SubVariantId", transaction.SubVariantId);
        cmd.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
        cmd.Parameters.AddWithValue("@Quantity", transaction.Quantity);
        cmd.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
        cmd.Parameters.AddWithValue("@Notes", (object?)transaction.Notes ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
        _logger.LogInformation("Stock transaction recorded: {Type} {Qty} for SubVariant {SubId}",
            transaction.TransactionType, transaction.Quantity, transaction.SubVariantId);
    }
}