namespace WebApplication1.Dto;

public class ProductOrderDto
{
    
    public class ProductionDto
    {
        public string FinishedGoodCode { get; set; }
        public decimal QuantityProduced { get; set; }
        public DateTime? ProductionDate { get; set; }

    
    }

    public class MaterialUsageDto
    {
        public int ItemID { get; set; }
        public decimal QuantityUsed { get; set; }
    }
    public class ProductionResultDto
    {
        public string ProductionCode { get; set; }
        public DateTime ProductionDate { get; set; }
        public int ProductItemId { get; set; }
        public string ProductItemName { get; set; }
        public decimal QuantityProduced { get; set; }
    }




}