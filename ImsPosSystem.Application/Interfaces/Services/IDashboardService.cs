using ImsPosSystem.Application.DTOs.Reporting;

namespace ImsPosSystem.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDTO> GetSummaryAsync();
    Task<IEnumerable<StockAlertDTO>> GetStockAlertsAsync();
    Task<IEnumerable<TopProductDTO>> GetTopSoldProductsAsync(int count = 5);
    Task<IEnumerable<MonthlySalesTrendDTO>> GetMonthlySalesTrendAsync();
}
