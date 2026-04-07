using System;
using System.Collections.Generic;

namespace ImsPosSystem.Domain.Entities;

public partial class SupplierPayment
{
    public long PaymentId { get; set; }

    public int SupplierId { get; set; }

    public long PurchaseInvoiceId { get; set; }

    public DateOnly PaymentDate { get; set; }

    public decimal AmountPaid { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? ReferenceNumber { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
