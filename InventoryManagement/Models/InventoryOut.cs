using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class InventoryOut
{
    public int InventoryOutId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public DateTime DateOut { get; set; }

    public string DestinationType { get; set; } = null!;

    public string? ReferenceNo { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<InventoryOutDetail> InventoryOutDetails { get; set; } = new List<InventoryOutDetail>();

    public virtual ItemMaster Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
