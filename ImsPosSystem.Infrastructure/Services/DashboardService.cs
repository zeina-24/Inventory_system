using ImsPosSystem.Application.DTOs.Reporting;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDTO> GetSummaryAsync()
    {
        var totalRevenue = await _unitOfWork.SalesInvoices.Query()
            .SumAsync(s => (decimal?)(s.TotalAmount - s.TaxAmount)) ?? 0; // Exclude tax for revenue metric

        var totalCOGS = await _unitOfWork.SalesInvoices.Query()
            .SelectMany(s => s.SalesInvoiceLines)
            .SumAsync(l => (decimal?)(l.Quantity * (l.UnitCostSnapshot ?? 0))) ?? 0;

        var salesCount = await _unitOfWork.SalesInvoices.CountAsync();
        var activeProducts = await _unitOfWork.Products.CountAsync(p => p.IsActive);

        var inventoryValue = await _unitOfWork.StockLedgers.Query()
            .Include(s => s.Product)
            .SumAsync(s => (decimal?)(s.QtyOnHand * s.Product.BuyingPrice)) ?? 0;

        return new DashboardSummaryDTO
        {
            TotalRevenue = totalRevenue,
            TotalCOGS = totalCOGS,
            TotalSalesCount = salesCount,
            ActiveProductsCount = activeProducts,
            TotalInventoryValue = inventoryValue
        };
    }

    public async Task<IEnumerable<StockAlertDTO>> GetStockAlertsAsync()
    {
        var alerts = await _unitOfWork.StockLedgers.Query()
            .Include(s => s.Product)
            .Include(s => s.Location)
            .Where(s => s.QtyOnHand <= s.Product.ReorderLevel)
            .Select(s => new StockAlertDTO
            {
                ProductId = s.ProductId,
                ProductName = s.Product.ProductName,
                QtyOnHand = s.QtyOnHand,
                ReorderLevel = s.Product.ReorderLevel,
                LocationName = s.Location.Description
            })
            .ToListAsync();

        return alerts;
    }

    public async Task<IEnumerable<TopProductDTO>> GetTopSoldProductsAsync(int count = 5)
    {
        var topProducts = await _unitOfWork.SalesInvoices.Query()
            .SelectMany(s => s.SalesInvoiceLines)
            .GroupBy(l => l.Product.ProductName)
            .Select(g => new TopProductDTO
            {
                ProductName = g.Key,
                TotalQuantitySold = g.Sum(x => x.Quantity),
                TotalRevenueGenerated = g.Sum(x => x.LineTotal ?? 0)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(count)
            .ToListAsync();

        return topProducts;
    }

    public async Task<IEnumerable<MonthlySalesTrendDTO>> GetMonthlySalesTrendAsync()
    {
        // For simplicity, we fetch 12 latest months including current
        var cutoff = DateTime.UtcNow.AddMonths(-11);
        cutoff = new DateTime(cutoff.Year, cutoff.Month, 1);

        var salesTrend = await _unitOfWork.SalesInvoices.Query()
            .Where(s => s.InvoiceDate >= cutoff)
            .GroupBy(s => new { s.InvoiceDate.Year, s.InvoiceDate.Month })
            .Select(g => new MonthlySalesTrendDTO
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                SalesAmount = g.Sum(x => x.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return salesTrend;
    }
}
