using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class InventoryOutDetail
{
    public int OutDetailId { get; set; }

    public int InventoryOutId { get; set; }

    public int InventoryInId { get; set; }

    public int QuantityUsed { get; set; }

    public decimal UnitCost { get; set; }

    public virtual InventoryIn InventoryIn { get; set; } = null!;

    public virtual InventoryOut InventoryOut { get; set; } = null!;
}
