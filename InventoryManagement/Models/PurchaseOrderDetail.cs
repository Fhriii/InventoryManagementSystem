using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class PurchaseOrderDetail
{
    public int PodetailId { get; set; }

    public int PurchaseOrderId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual ItemMaster Item { get; set; } = null!;

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
