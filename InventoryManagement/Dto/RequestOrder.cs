namespace WebApplication1.Dto;

public class RequestOrders
{
    public class RequestDto
    {
        public string CustomerName { get; set; } 
        public string CustomerPhone { get; set; }
        
        public int Quantity { get; set; } 

        public string FinishedGoodName { get; set; }
        public string FinishedGoodUnit { get; set; }

        public List<RequestItemDto> Items { get; set; } = new List<RequestItemDto>();
    }

    public class RequestItemDto
    {
        public string ItemCode { get; set; }
        public string? ItemName { get; set; } 
        public string? Unit { get; set; }

        public decimal Quantity { get; set; }
    }

    public class RequestOrderItemExist
    {
        public string CustomerName { get; set; } 
        public string CustomerPhone { get; set; }
        public string? ItemCode { get; set; }
        public int Quantity { get; set; } 
    }

}