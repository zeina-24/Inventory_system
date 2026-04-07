using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImsPosSystem.Domain.Entities;

public partial class StockLedger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LedgerId { get; set; }

    public int ProductId { get; set; }

    public int LocationId { get; set; }

    public int QtyOnHand { get; set; }

    public DateTime LastMovedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual StorageLocation Location { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
