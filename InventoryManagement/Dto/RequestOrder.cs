namespace WebApplication1.Dto;

public class RequestOrders
{
    public class RequestDto
    {
        public string? RequestNumber { get; set; }
        public int UserID { get; set; }
        public DateTime RequestDate { get; set; }
        public string CustomerName { get; set; } 
        public string CustomerPhone { get; set; }

        // Kalau pakai BOM existing
        public int? FinishedGoodID { get; set; }
        public int? Quantity { get; set; } 

        // Kalau input manual (BOM baru)
        public string? FinishedGoodCode { get; set; }
        public string? FinishedGoodName { get; set; }
        public string? FinishedGoodUnit { get; set; }

        public List<RequestItemDto> Items { get; set; } = new List<RequestItemDto>();
    }

    public class RequestItemDto
    {
        public string ItemCode { get; set; }
        public string? ItemName { get; set; } 
        public string? ItemType { get; set; } 
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
    }
    

}