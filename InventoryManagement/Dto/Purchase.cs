namespace WebApplication1.Dto;

public class Purchase
{
    public class PurchaseOrderDto
    { 
        public int SupplierID { get; set; }
        public int UserID { get; set; }
        public DateOnly? PODate { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; }
    }

    public class PurchaseOrderItemDto
    {
        public string ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Description { get; set; }
        public int? MinStock { get; set; }
    }

}