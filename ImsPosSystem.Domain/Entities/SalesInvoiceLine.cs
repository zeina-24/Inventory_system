using System;
using System.Collections.Generic;

namespace ImsPosSystem.Domain.Entities;

public partial class SalesInvoiceLine
{
    public long LineId { get; set; }

    public long SalesInvoiceId { get; set; }

    public int ProductId { get; set; }

    public int LocationId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitSellingPrice { get; set; }

    public decimal? UnitCostSnapshot { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal? LineSubTotal { get; set; }

    public decimal? LineDiscount { get; set; }

    public decimal? LineTotal { get; set; }

    public virtual StorageLocation Location { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual SalesInvoice SalesInvoice { get; set; } = null!;
}
