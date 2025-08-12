using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual ICollection<InventoryIn> InventoryIns { get; set; } = new List<InventoryIn>();

    public virtual ICollection<InventoryOut> InventoryOuts { get; set; } = new List<InventoryOut>();

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<RequestOrder> RequestOrders { get; set; } = new List<RequestOrder>();
}
