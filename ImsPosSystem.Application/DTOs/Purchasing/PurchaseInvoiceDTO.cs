using System.ComponentModel.DataAnnotations;

namespace ImsPosSystem.Application.DTOs.Purchasing;

public class PurchaseInvoiceDTO
{
    public long PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public DateOnly InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public List<PurchaseInvoiceLineDTO> Lines { get; set; } = new();
}

public class PurchaseInvoiceLineDTO
{
    public long LineId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitBuyingPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineSubTotal { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreatePurchaseInvoiceDTO
{
    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = null!;

    [Required]
    public int SupplierId { get; set; }

    public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one line item is required.")]
    public List<CreatePurchaseInvoiceLineDTO> Lines { get; set; } = new();
}

public class CreatePurchaseInvoiceLineDTO
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int LocationId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
    public decimal UnitBuyingPrice { get; set; }

    [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
    public decimal DiscountPercent { get; set; }
}
