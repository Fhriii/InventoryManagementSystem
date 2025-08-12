using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class InventoryIn
{
    public int InventoryInId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public DateTime DateIn { get; set; }

    public string SourceType { get; set; } = null!;

    public string? ReferenceNo { get; set; }

    public int UserId { get; set; }

    public string? BatchNumber { get; set; }

    public int? RemainingQty { get; set; }

    public virtual ICollection<InventoryOutDetail> InventoryOutDetails { get; set; } = new List<InventoryOutDetail>();

    public virtual ItemMaster Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
