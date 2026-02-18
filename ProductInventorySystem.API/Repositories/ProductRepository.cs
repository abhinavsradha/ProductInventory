using Microsoft.Data.SqlClient;
using System.Data;
using ProductInventorySystem.API.Data;
using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Models;
using ProductInventorySystem.API.Repositories.Interfaces;

namespace ProductInventorySystem.API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _factory;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(DbConnectionFactory factory, ILogger<ProductRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<bool> ProductCodeExistsAsync(string productCode)
    {
        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM Products WHERE ProductCode = @Code", conn);
        cmd.Parameters.AddWithValue("@Code", productCode);
        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public async Task<Guid> CreateProductAsync(Product product)
    {
        const string sql = @"
            INSERT INTO Products 
                (Id, ProductCode, ProductName, ProductImage, CreatedDate, UpdatedDate,
                 CreatedUser, IsFavourite, Active, HSNCode, TotalStock)
            VALUES 
                (@Id, @ProductCode, @ProductName, @ProductImage, @CreatedDate, @UpdatedDate,
                 @CreatedUser, @IsFavourite, @Active, @HSNCode, @TotalStock)";

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(sql, conn);

        // Use explicit SqlParameter for every field — never mix AddWithValue + SqlParameter for same param
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = product.Id });
        cmd.Parameters.Add(new SqlParameter("@ProductCode", SqlDbType.NVarChar, 50) { Value = product.ProductCode });
        cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 200) { Value = product.ProductName });
        cmd.Parameters.Add(new SqlParameter("@ProductImage", SqlDbType.VarBinary, -1) { Value = (object?)product.ProductImage ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTimeOffset) { Value = product.CreatedDate });
        cmd.Parameters.Add(new SqlParameter("@UpdatedDate", SqlDbType.DateTimeOffset) { Value = product.UpdatedDate });
        cmd.Parameters.Add(new SqlParameter("@CreatedUser", SqlDbType.UniqueIdentifier) { Value = product.CreatedUser });
        cmd.Parameters.Add(new SqlParameter("@IsFavourite", SqlDbType.Bit) { Value = product.IsFavourite });
        cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.Bit) { Value = product.Active });
        cmd.Parameters.Add(new SqlParameter("@HSNCode", SqlDbType.NVarChar, 100) { Value = product.HSNCode });
        cmd.Parameters.Add(new SqlParameter("@TotalStock", SqlDbType.Decimal) { Value = product.TotalStock });

        await cmd.ExecuteNonQueryAsync();

        _logger.LogInformation("Product created: {ProductId}", product.Id);
        return product.Id;
    }

    public async Task<Guid> CreateVariantAsync(Variant variant)
    {
        const string sql = @"
            INSERT INTO Variants (Id, ProductId, Name, CreatedDate)
            VALUES (@Id, @ProductId, @Name, @CreatedDate)";

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", variant.Id);
        cmd.Parameters.AddWithValue("@ProductId", variant.ProductId);
        cmd.Parameters.AddWithValue("@Name", variant.Name);
        cmd.Parameters.AddWithValue("@CreatedDate", variant.CreatedDate);
        await cmd.ExecuteNonQueryAsync();
        return variant.Id;
    }

    public async Task<Guid> CreateSubVariantAsync(SubVariant subVariant)
    {
        const string sql = @"
            INSERT INTO SubVariants (Id, VariantId, ProductId, OptionValue, Stock, SKU, CreatedDate)
            VALUES (@Id, @VariantId, @ProductId, @OptionValue, @Stock, @SKU, @CreatedDate)";

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", subVariant.Id);
        cmd.Parameters.AddWithValue("@VariantId", subVariant.VariantId);
        cmd.Parameters.AddWithValue("@ProductId", subVariant.ProductId);
        cmd.Parameters.AddWithValue("@OptionValue", subVariant.OptionValue);
        cmd.Parameters.AddWithValue("@Stock", subVariant.Stock);
        cmd.Parameters.AddWithValue("@SKU", (object?)subVariant.SKU ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedDate", subVariant.CreatedDate);
        await cmd.ExecuteNonQueryAsync();
        return subVariant.Id;
    }

    public async Task<PagedResult<ProductListDto>> GetProductsAsync(int page, int pageSize, bool? active)
    {
        var whereClause = active.HasValue ? "WHERE p.Active = @Active" : "";
        var countSql = $"SELECT COUNT(*) FROM Products p {whereClause}";

        var productSql = $@"
            SELECT p.Id, p.ProductCode, p.ProductName, p.IsFavourite, p.Active,
                   p.HSNCode, p.TotalStock, p.CreatedDate
            FROM Products p
            {whereClause}
            ORDER BY p.CreatedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var conn = _factory.CreateConnection();
        await conn.OpenAsync();

        // Get total count
        using var countCmd = new SqlCommand(countSql, conn);
        if (active.HasValue) countCmd.Parameters.AddWithValue("@Active", active.Value);
        var totalCount = (int)(await countCmd.ExecuteScalarAsync())!;

        // Get paged products
        var products = new List<ProductListDto>();
        using var prodCmd = new SqlCommand(productSql, conn);
        if (active.HasValue) prodCmd.Parameters.AddWithValue("@Active", active.Value);
        prodCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        prodCmd.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = await prodCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new ProductListDto
            {
                Id = reader.GetGuid(0),
                ProductCode = reader.GetString(1),
                ProductName = reader.GetString(2),
                IsFavourite = reader.GetBoolean(3),
                Active = reader.GetBoolean(4),
                HSNCode = reader.GetString(5),
                TotalStock = reader.GetDecimal(6),
                CreatedDate = reader.GetDateTimeOffset(7)
            });
        }
        await reader.CloseAsync();

        // Get variants and subvariants for the current page of products
        if (products.Any())
        {
            var variantDict = new Dictionary<Guid, VariantListDto>();
            var productVariantMap = new Dictionary<Guid, List<VariantListDto>>();

            // Build parameterized IN clause — one param per product id
            var inParams = string.Join(",", products.Select((_, i) => $"@pid{i}"));
            var variantSql = $@"
                SELECT v.Id, v.ProductId, v.Name,
                       sv.Id AS SubId, sv.OptionValue, sv.Stock, sv.SKU
                FROM Variants v
                INNER JOIN SubVariants sv ON sv.VariantId = v.Id
                WHERE v.ProductId IN ({inParams})
                ORDER BY v.ProductId, v.Id";

            using var varCmd = new SqlCommand(variantSql, conn);
            for (int i = 0; i < products.Count; i++)
                varCmd.Parameters.AddWithValue($"@pid{i}", products[i].Id);

            using var vReader = await varCmd.ExecuteReaderAsync();
            while (await vReader.ReadAsync())
            {
                var variantId = vReader.GetGuid(0);
                var productId = vReader.GetGuid(1);

                if (!variantDict.TryGetValue(variantId, out var variant))
                {
                    variant = new VariantListDto
                    {
                        Id = variantId,
                        Name = vReader.GetString(2)
                    };
                    variantDict[variantId] = variant;

                    if (!productVariantMap.ContainsKey(productId))
                        productVariantMap[productId] = new List<VariantListDto>();

                    productVariantMap[productId].Add(variant);
                }

                variant.SubVariants.Add(new SubVariantListDto
                {
                    Id = vReader.GetGuid(3),
                    OptionValue = vReader.GetString(4),
                    Stock = vReader.GetDecimal(5),
                    SKU = vReader.IsDBNull(6) ? null : vReader.GetString(6)
                });
            }

            foreach (var p in products)
                if (productVariantMap.TryGetValue(p.Id, out var variants))
                    p.Variants = variants;
        }

        return new PagedResult<ProductListDto>
        {
            Items = products,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}