using WebApplication1.Models;

namespace WebApplication1.Dto;

public class DeliveryResponseDto
{
    public string DeliveryNumber { get; set; } = null!;

    public string RequestNumber { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public string? DeliveryStatus { get; set; }
    
    public virtual RequestOrder Request { get; set; } = null!;
}
