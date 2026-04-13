using System.ComponentModel.DataAnnotations;

namespace ImsPosSystem.Application.DTOs.POS;

public class SalesInvoiceDTO
{
    public long SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? CashierName { get; set; }
    public string? Notes { get; set; }
    public List<SalesInvoiceLineDTO> Lines { get; set; } = new();
}

public class SalesInvoiceLineDTO
{
    public long LineId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitSellingPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineSubTotal { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateSalesInvoiceDTO
{
    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = null!;

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    [Required]
    [StringLength(30)]
    public string PaymentMethod { get; set; } = "CASH";

    public string? CashierName { get; set; }
    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one sale item is required.")]
    public List<CreateSalesInvoiceLineDTO> Lines { get; set; } = new();
}

public class CreateSalesInvoiceLineDTO
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int LocationId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
    public decimal DiscountPercent { get; set; }
}
