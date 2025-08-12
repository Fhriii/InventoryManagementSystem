namespace WebApplication1.Dto;

public class ProductOrderDto
{
    
    public class ProductionDto
    {
        public int FinishedGoodID { get; set; }
        public decimal QuantityProduced { get; set; }
        public DateTime ProductionDate { get; set; }

        // Kalau null → berarti pakai BOM existing
        public List<MaterialUsageDto>? Materials { get; set; }
    }

    public class MaterialUsageDto
    {
        public int ItemID { get; set; }
        public decimal QuantityUsed { get; set; }
    }
    public class ProductionResultDto
    {
        public int ProductionId { get; set; }
        public DateTime ProductionDate { get; set; }
        public int ProductItemId { get; set; }
        public string ProductItemName { get; set; }
        public int QuantityProduced { get; set; }
    }




}