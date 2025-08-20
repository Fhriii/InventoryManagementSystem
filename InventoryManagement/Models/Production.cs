using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Production
{
    public int ProductionId { get; set; }

    public DateTime ProductionDate { get; set; }

    public int ProductItemId { get; set; }

    public decimal QuantityProduced { get; set; }

    public string? ReferenceNo { get; set; }

    public int UserId { get; set; }

    public virtual ItemMaster ProductItem { get; set; } = null!;

    public virtual ICollection<ProductionMaterialUsage> ProductionMaterialUsages { get; set; } = new List<ProductionMaterialUsage>();

    public virtual User User { get; set; } = null!;
}
