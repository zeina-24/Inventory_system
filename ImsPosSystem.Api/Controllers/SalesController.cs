using ImsPosSystem.Application.DTOs.POS;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace ImsPosSystem.Api.Controllers;

[Authorize(Roles = "Admin, Cashier")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    /// <summary>Get all sales transactions</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDTO>>> GetSales()
        => Ok(await _salesService.GetAllSalesAsync());

    /// <summary>Get a specific sale by ID</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SalesInvoiceDTO>> GetSale(long id)
    {
        var result = await _salesService.GetSaleByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Process a new sale</summary>
    /// <remarks>
    /// This will check stock availability, decrement stock levels, and capture 
    /// the cost of items at the time of sale.
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<SalesInvoiceDTO>> CreateSale([FromBody] CreateSalesInvoiceDTO dto)
    {
        try
        {
            var result = await _salesService.CreateSaleAsync(dto);
            return CreatedAtAction(nameof(GetSale), new { id = result.SalesInvoiceId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
