using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class RequestOrder
{
    public int RequestId { get; set; }

    public string RequestNumber { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? CustomerPhone { get; set; }

    public string? Status { get; set; }

    public int UserId { get; set; }

    public decimal Quantity { get; set; }

    public int ItemId { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ItemMaster? Item { get; set; }

    public virtual ICollection<RequestOrderDetail> RequestOrderDetails { get; set; } = new List<RequestOrderDetail>();

    public virtual User User { get; set; } = null!;
}
