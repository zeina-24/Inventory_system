using ImsPosSystem.Application.DTOs.Warehouse;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImsPosSystem.Api.Controllers;

[ApiController]
[Route("api/warehouse/locations")]
[Produces("application/json")]
public class StorageLocationsController : ControllerBase
{
    private readonly IStorageLocationService _service;

    public StorageLocationsController(IStorageLocationService service) => _service = service;

    /// <summary>Get all storage locations with optional block/status filter</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StorageLocationDto>>> GetAll(
        [FromQuery] string? block, [FromQuery] bool? isActive)
        => Ok(await _service.GetAllLocationsAsync(block, isActive));

    /// <summary>Get a storage location by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StorageLocationDto>> GetById(int id)
    {
        var result = await _service.GetLocationByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a storage location</summary>
    [HttpPost]
    public async Task<ActionResult<StorageLocationDto>> Create([FromBody] CreateStorageLocationDto dto)
    {
        var result = await _service.CreateLocationAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.LocationId }, result);
    }

    /// <summary>Update a storage location</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<StorageLocationDto>> Update(int id, [FromBody] UpdateStorageLocationDto dto)
        => Ok(await _service.UpdateLocationAsync(id, dto));

    /// <summary>Delete a storage location (blocked if stock or transactions exist)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteLocationAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/warehouse/stock")]
[Produces("application/json")]
public class StockLedgerController : ControllerBase
{
    private readonly IStockLedgerService _service;

    public StockLedgerController(IStockLedgerService service) => _service = service;

    /// <summary>Get stock ledger with optional filters</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockLedgerDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? block,
        [FromQuery] string? aisle,
        [FromQuery] bool lowStockOnly = false)
        => Ok(await _service.GetStockAsync(search, block, aisle, lowStockOnly));

    /// <summary>Get all stock entries for a product</summary>
    [HttpGet("by-product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<StockLedgerDto>>> ByProduct(int productId)
        => Ok(await _service.GetStockByProductAsync(productId));

    /// <summary>Get all stock entries for a location</summary>
    [HttpGet("by-location/{locationId:int}")]
    public async Task<ActionResult<IEnumerable<StockLedgerDto>>> ByLocation(int locationId)
        => Ok(await _service.GetStockByLocationAsync(locationId));

    /// <summary>Transfer stock between bins (admin)</summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] StockTransferDto dto)
    {
        await _service.TransferStockAsync(dto);
        return NoContent();
    }
}
