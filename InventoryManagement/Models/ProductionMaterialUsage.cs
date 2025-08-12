using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class ProductionMaterialUsage
{
    public int UsageId { get; set; }

    public int ProductionId { get; set; }

    public int RawMaterialItemId { get; set; }

    public int QuantityUsed { get; set; }

    public virtual Production Production { get; set; } = null!;

    public virtual ItemMaster RawMaterialItem { get; set; } = null!;
}
