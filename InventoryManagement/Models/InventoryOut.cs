using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class InventoryOut
{
    public int InventoryOutId { get; set; }

    public int? InventoryId { get; set; }

    public decimal? UnitCost { get; set; }

    public int QuantityUsed { get; set; }

    public DateTime DateOut { get; set; }

    public string DestinationType { get; set; } = null!;

    public string? ReferenceNo { get; set; }

    public int UserId { get; set; }

    public virtual Inventory? Inventory { get; set; }

    public virtual User User { get; set; } = null!;
}
