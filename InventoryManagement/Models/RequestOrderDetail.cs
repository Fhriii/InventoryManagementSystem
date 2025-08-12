using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class RequestOrderDetail
{
    public int RequestDetailId { get; set; }

    public int RequestId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual ItemMaster Item { get; set; } = null!;

    public virtual RequestOrder Request { get; set; } = null!;
}
