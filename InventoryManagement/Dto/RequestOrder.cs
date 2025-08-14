namespace WebApplication1.Dto;

public class RequestOrders
{
    public class RequestDto
    {
        public string? RequestNumber { get; set; }
        public string CustomerName { get; set; } 
        public string CustomerPhone { get; set; }

        // Kalau pakai BOM existing
        public string? ItemCode { get; set; }
        public int? Quantity { get; set; } 

        // Kalau input manual (BOM baru)
        public string? FinishedGoodName { get; set; }
        public string? FinishedGoodUnit { get; set; }

        public List<RequestItemDto> Items { get; set; } = new List<RequestItemDto>();
    }

    public class RequestItemDto
    {
        public string ItemCode { get; set; }
        public string? ItemName { get; set; } 
        public string? Unit { get; set; }

        public decimal Quantity { get; set; }
    }
    

}