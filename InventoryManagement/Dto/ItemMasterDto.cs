using WebApplication1.Models;

namespace WebApplication1.Dto;

public class ItemMasterDto
{
    public string ItemCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string ItemType { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public int? CurrentStock { get; set; }

    public int? MinStock { get; set; }

    public string? Description { get; set; }
    
    
}