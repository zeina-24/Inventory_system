namespace ImsPosSystem.Application.DTOs.Reporting;

public class DashboardSummaryDTO
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCOGS { get; set; } // Cost of Goods Sold
    public decimal TotalProfit => TotalRevenue - TotalCOGS;
    public int TotalSalesCount { get; set; }
    public int ActiveProductsCount { get; set; }
    public decimal TotalInventoryValue { get; set; } // Stock * BuyingPrice
}

public class StockAlertDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int QtyOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public string LocationName { get; set; } = null!;
}

public class TopProductDTO
{
    public string ProductName { get; set; } = null!;
    public decimal TotalQuantitySold { get; set; }
    public decimal TotalRevenueGenerated { get; set; }
}

public class MonthlySalesTrendDTO
{
    public string Month { get; set; } = null!;
    public decimal SalesAmount { get; set; }
    public int OrderCount { get; set; }
}
