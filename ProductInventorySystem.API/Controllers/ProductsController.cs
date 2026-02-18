using Microsoft.AspNetCore.Mvc;
using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Services.Interfaces;

namespace ProductInventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Create a new product with variants and sub-variants</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for CreateProduct");
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", ModelState));
        }

        try
        {
            var id = await _service.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProducts), null,
                ApiResponse<object>.Ok(new { id }, "Product created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>List all products with pagination</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? active = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        _logger.LogInformation("Listing products - Page:{Page} Size:{Size}", page, pageSize);
        var result = await _service.GetProductsAsync(page, pageSize, active);
        return Ok(ApiResponse<PagedResult<ProductListDto>>.Ok(result));
    }
}