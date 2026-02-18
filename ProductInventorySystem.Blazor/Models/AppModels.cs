namespace ProductInventorySystem.Blazor.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public bool IsFavourite { get; set; }
    public bool Active { get; set; }
    public string HSNCode { get; set; } = "";
    public decimal TotalStock { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public List<VariantListDto> Variants { get; set; } = new();
}

public class VariantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public List<SubVariantListDto> SubVariants { get; set; } = new();
}

public class SubVariantListDto
{
    public Guid Id { get; set; }
    public string OptionValue { get; set; } = "";
    public decimal Stock { get; set; }
    public string? SKU { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = "";
    public string HSNCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public Guid CreatedUser { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public bool IsFavourite { get; set; }
    public List<VariantRequest> Variants { get; set; } = new();
}

public class VariantRequest
{
    public string Name { get; set; } = "";
    public List<string> Options { get; set; } = new();
}

public class StockRequest
{
    public Guid ProductId { get; set; }
    public Guid SubVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class ProductFormModel
{
    public string Name { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string HSNCode { get; set; } = "";
    public bool IsFavourite { get; set; }
    public List<VariantFormModel> Variants { get; set; } = new() { new() };

    public Dictionary<string, string> Validate()
    {
        var e = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(Name)) e["Name"] = "Product name is required.";
        if (string.IsNullOrWhiteSpace(ProductCode)) e["ProductCode"] = "Product code is required.";
        if (string.IsNullOrWhiteSpace(HSNCode)) e["HSNCode"] = "HSN code is required.";
        if (!Variants.Any()) e["Variants"] = "Add at least one variant.";
        for (int i = 0; i < Variants.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(Variants[i].Name))
                e[$"V{i}Name"] = "Variant name is required.";
            if (!Variants[i].Options.Any())
                e[$"V{i}Opts"] = "Add at least one option.";
        }
        return e;
    }
}

public class VariantFormModel
{
    public string Name { get; set; } = "";
    public List<string> Options { get; set; } = new();
    public string NewOption { get; set; } = "";
}