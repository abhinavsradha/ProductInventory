using System.Net.Http.Json;
using System.Text.Json;
using ProductInventorySystem.Blazor.Models;

namespace ProductInventorySystem.Blazor.Services;

public interface IProductService
{
    Task<(PagedResult<ProductListDto>? Data, string? Error)> GetProductsAsync(
        int page = 1, int pageSize = 10, bool? active = null);
    Task<(Guid? Id, string? Error)> CreateProductAsync(CreateProductRequest req);
}

public interface IStockService
{
    Task<string?> AddStockAsync(StockRequest req);
    Task<string?> RemoveStockAsync(StockRequest req);
}

public class ProductService : IProductService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _o = new() { PropertyNameCaseInsensitive = true };

    public ProductService(HttpClient http) => _http = http;

    public async Task<(PagedResult<ProductListDto>? Data, string? Error)> GetProductsAsync(
        int page = 1, int pageSize = 10, bool? active = null)
    {
        try
        {
            var url = $"api/Products?page={page}&pageSize={pageSize}";
            if (active.HasValue) url += $"&active={active.Value.ToString().ToLower()}";
            var res = await _http.GetFromJsonAsync<ApiResponse<PagedResult<ProductListDto>>>(url, _o);
            return res?.Success == true ? (res.Data, null) : (null, res?.Message ?? "Failed.");
        }
        catch (Exception ex) { return (null, $"Connection error: {ex.Message}"); }
    }

    public async Task<(Guid? Id, string? Error)> CreateProductAsync(CreateProductRequest req)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Products", req);
            var body = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {
                var r = JsonSerializer.Deserialize<ApiResponse<JsonElement>>(body, _o);
                if (r?.Data is JsonElement d && d.TryGetProperty("id", out var id))
                    return (Guid.Parse(id.GetString()!), null);
                return (Guid.NewGuid(), null);
            }
            var err = JsonSerializer.Deserialize<ApiResponse<object>>(body, _o);
            return (null, err?.Message ?? $"Error {(int)res.StatusCode}");
        }
        catch (Exception ex) { return (null, $"Connection error: {ex.Message}"); }
    }
}

public class StockService : IStockService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _o = new() { PropertyNameCaseInsensitive = true };

    public StockService(HttpClient http) => _http = http;

    public async Task<string?> AddStockAsync(StockRequest req)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Stock/purchase", req);
            if (res.IsSuccessStatusCode) return null;
            var err = JsonSerializer.Deserialize<ApiResponse<object>>(await res.Content.ReadAsStringAsync(), _o);
            return err?.Message ?? "Failed to add stock.";
        }
        catch (Exception ex) { return $"Connection error: {ex.Message}"; }
    }

    public async Task<string?> RemoveStockAsync(StockRequest req)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/Stock/sale", req);
            if (res.IsSuccessStatusCode) return null;
            var err = JsonSerializer.Deserialize<ApiResponse<object>>(await res.Content.ReadAsStringAsync(), _o);
            return err?.Message ?? "Failed to remove stock.";
        }
        catch (Exception ex) { return $"Connection error: {ex.Message}"; }
    }
}