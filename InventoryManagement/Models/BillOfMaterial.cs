using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class BillOfMaterial
{
    public int Bomid { get; set; }

    public int FinishedGoodId { get; set; }

    public int RawMaterialId { get; set; }

    public decimal QuantityRequired { get; set; }

    public string Unit { get; set; } = null!;

    public virtual ItemMaster FinishedGood { get; set; } = null!;

    public virtual ItemMaster RawMaterial { get; set; } = null!;
}
