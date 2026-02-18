using Microsoft.AspNetCore.Mvc;
using ProductInventorySystem.API.DTOs;
using ProductInventorySystem.API.Services.Interfaces;

namespace ProductInventorySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _service;
    private readonly ILogger<StockController> _logger;

    public StockController(IStockService service, ILogger<StockController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Add stock (Purchase)</summary>
    [HttpPost("purchase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStock([FromBody] StockRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", ModelState));

        try
        {
            await _service.AddStockAsync(dto);
            return Ok(ApiResponse<object>.Ok(null, "Stock added successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Remove stock (Sale)</summary>
    [HttpPost("sale")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStock([FromBody] StockRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed.", ModelState));

        try
        {
            await _service.RemoveStockAsync(dto);
            return Ok(ApiResponse<object>.Ok(null, "Stock removed successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}