using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImsPosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    /// <summary>Get all products with optional search/filter</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? subcategoryId,
        [FromQuery] bool? isActive)
        => Ok(await _service.GetAllProductsAsync(search, categoryId, subcategoryId, isActive));

    /// <summary>Get a product by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var result = await _service.GetProductByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Product lookup for dropdowns and POS search</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<IEnumerable<ProductLookupDto>>> Lookup([FromQuery] string? search)
        => Ok(await _service.GetProductLookupsAsync(search));

    /// <summary>Create a product</summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var result = await _service.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
    }

    /// <summary>Update a product</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto dto)
        => Ok(await _service.UpdateProductAsync(id, dto));

    /// <summary>Soft-deactivate a product (IsActive = false)</summary>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateProductAsync(id);
        return NoContent();
    }

    /// <summary>Hard-delete a product (blocked if transactions exist)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteProductAsync(id);
        return NoContent();
    }
}
