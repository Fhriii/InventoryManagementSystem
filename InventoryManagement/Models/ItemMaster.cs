using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class ItemMaster
{
    public int ItemId { get; set; }

    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string ItemType { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int? CurrentStock { get; set; }

    public int? MinStock { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BillOfMaterial> BillOfMaterialFinishedGoods { get; set; } = new List<BillOfMaterial>();

    public virtual ICollection<BillOfMaterial> BillOfMaterialRawMaterials { get; set; } = new List<BillOfMaterial>();

    public virtual ICollection<InventoryIn> InventoryIns { get; set; } = new List<InventoryIn>();

    public virtual ICollection<InventoryOut> InventoryOuts { get; set; } = new List<InventoryOut>();

    public virtual ICollection<ProductionMaterialUsage> ProductionMaterialUsages { get; set; } = new List<ProductionMaterialUsage>();

    public virtual ICollection<Production> Productions { get; set; } = new List<Production>();

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<RequestOrderDetail> RequestOrderDetails { get; set; } = new List<RequestOrderDetail>();
}
