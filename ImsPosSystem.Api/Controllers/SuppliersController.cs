using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace ImsPosSystem.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>Get all suppliers</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers()
        => Ok(await _supplierService.GetAllSuppliersAsync());

    /// <summary>Get a single supplier by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDTO>> GetSupplier(int id)
    {
        var result = await _supplierService.GetSupplierByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new supplier</summary>
    [HttpPost]
    public async Task<ActionResult<SupplierDTO>> CreateSupplier([FromBody] CreateSupplierDTO dto)
    {
        var result = await _supplierService.CreateSupplierAsync(dto);
        return CreatedAtAction(nameof(GetSupplier), new { id = result.SupplierId }, result);
    }

    /// <summary>Update an existing supplier</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SupplierDTO>> UpdateSupplier(int id, [FromBody] UpdateSupplierDTO dto)
        => Ok(await _supplierService.UpdateSupplierAsync(id, dto));

    /// <summary>Delete a supplier</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        await _supplierService.DeleteSupplierAsync(id);
        return NoContent();
    }
}
