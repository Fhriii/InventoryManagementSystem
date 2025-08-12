using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Delivery
{
    public int DeliveryId { get; set; }

    public string DeliveryNumber { get; set; } = null!;

    public int RequestId { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public string? DeliveryStatus { get; set; }

    public int UserId { get; set; }

    public virtual RequestOrder Request { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
