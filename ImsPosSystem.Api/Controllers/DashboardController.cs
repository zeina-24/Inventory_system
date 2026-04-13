using ImsPosSystem.Application.DTOs.Reporting;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace ImsPosSystem.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Get high-level business KPIs</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDTO>> GetSummary()
        => Ok(await _dashboardService.GetSummaryAsync());

    /// <summary>Get products that are below reorder level</summary>
    [HttpGet("stock-alerts")]
    public async Task<ActionResult<IEnumerable<StockAlertDTO>>> GetStockAlerts()
        => Ok(await _dashboardService.GetStockAlertsAsync());

    /// <summary>Get top selling products by quantity</summary>
    [HttpGet("top-selling")]
    public async Task<ActionResult<IEnumerable<TopProductDTO>>> GetTopSelling([FromQuery] int count = 5)
        => Ok(await _dashboardService.GetTopSoldProductsAsync(count));

    /// <summary>Get monthly sales totals for the last 12 months</summary>
    [HttpGet("sales-trend")]
    public async Task<ActionResult<IEnumerable<MonthlySalesTrendDTO>>> GetSalesTrend()
        => Ok(await _dashboardService.GetMonthlySalesTrendAsync());
}
