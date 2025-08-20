using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class InventoryIn
{
    public int InventoryInId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string SourceType { get; set; } = null!;

    public int? ItemId { get; set; }

    public decimal? UnitCost { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? ReferenceNo { get; set; }

    public decimal Quantity { get; set; }

    public int UserId { get; set; }

    public virtual ItemMaster? Item { get; set; }

    public virtual User User { get; set; } = null!;
}
