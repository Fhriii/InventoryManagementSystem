using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int? Quantity { get; set; }

    public int? ItemId { get; set; }

    public decimal? UnitCost { get; set; }

    public string? BatchNumber { get; set; }

    public DateTime? DateIn { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<InventoryOut> InventoryOuts { get; set; } = new List<InventoryOut>();

    public virtual ItemMaster? Item { get; set; }
}
