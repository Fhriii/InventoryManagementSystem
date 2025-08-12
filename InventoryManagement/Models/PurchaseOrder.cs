using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }

    public string Ponumber { get; set; } = null!;

    public int SupplierId { get; set; }

    public int UserId { get; set; }

    public DateOnly Podate { get; set; }

    public string? Status { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
