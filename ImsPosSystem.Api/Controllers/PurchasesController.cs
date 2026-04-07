using ImsPosSystem.Application.DTOs.Purchasing;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImsPosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    /// <summary>Get all purchase invoices</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceDTO>>> GetPurchases()
        => Ok(await _purchaseService.GetAllPurchasesAsync());

    /// <summary>Get a single purchase invoice by ID</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchaseInvoiceDTO>> GetPurchase(long id)
    {
        var result = await _purchaseService.GetPurchaseByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Get purchase invoices by supplier</summary>
    [HttpGet("supplier/{supplierId:int}")]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceDTO>>> GetPurchasesBySupplier(int supplierId)
        => Ok(await _purchaseService.GetPurchasesBySupplierAsync(supplierId));

    /// <summary>Create and process a new purchase invoice</summary>
    /// <remarks>This will automatically update the stock levels and product buying prices.</remarks>
    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceDTO>> CreatePurchase([FromBody] CreatePurchaseInvoiceDTO dto)
    {
        var result = await _purchaseService.CreatePurchaseInvoiceAsync(dto);
        return CreatedAtAction(nameof(GetPurchase), new { id = result.PurchaseInvoiceId }, result);
    }
}
