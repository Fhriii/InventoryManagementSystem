using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IProductionOrderService
{
    Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(ProductOrderDto.ProductionDto dto,int userId);
    
}

public class ProductionService:IProductionOrderService
{
    private readonly InventoryManagementContext _context;

    public ProductionService(InventoryManagementContext context)
    {
        _context = context;
    }

    public async Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(ProductOrderDto.ProductionDto dto,int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            var fg = await _context.ItemMasters.FirstOrDefaultAsync(i => i.ItemId == dto.FinishedGoodID && i.ItemType == "FinishedGoods");
            if (fg == null) throw new Exception("Finished good tidak ditemukan.");

            List<ProductOrderDto.MaterialUsageDto> materials;
            if (dto.Materials == null || !dto.Materials.Any())
            {
                materials = await _context.BillOfMaterials
                    .Where(b => b.FinishedGoodId == dto.FinishedGoodID)
                    .Select(b => new ProductOrderDto.MaterialUsageDto
                    {
                        ItemID = b.FinishedGoodId,
                        QuantityUsed = b.QuantityRequired * dto.QuantityProduced
                    }).ToListAsync();

                if (!materials.Any())
                    throw new Exception("BOM untuk produk ini belum ada.");
            }
            else
            {
                materials = dto.Materials;
            }

            foreach (var m in materials)
            {
                var item = await _context.ItemMasters.FirstOrDefaultAsync(i => i.ItemId == m.ItemID);
                if (item == null) throw new Exception($"Item ID {m.ItemID} tidak ditemukan.");
                if (item.CurrentStock < m.QuantityUsed)
                    throw new Exception($"Stok tidak cukup untuk item {item.ItemName}.");
            }
            var prodCount = await _context.Productions.CountAsync();

            var prod = new Production
            {
                ProductionDate = dto.ProductionDate,
                ProductItemId = dto.FinishedGoodID,
                QuantityProduced = (int)dto.QuantityProduced ,
                UserId = userId,
                ReferenceNo = $"PROD00{prodCount}/{DateTime.Now.Year}"
            };

            _context.Productions.Add(prod);
            await _context.SaveChangesAsync();

            foreach (var m in materials)
            {
                _context.ProductionMaterialUsages.Add(new ProductionMaterialUsage
                {
                    ProductionId = prod.ProductionId,
                    RawMaterialItemId = m.ItemID,
                    QuantityUsed = (int)m.QuantityUsed,
                });
                var invOutCount = await _context.InventoryOuts.Where(u=>u.DestinationType == "Production").CountAsync();

                _context.InventoryOuts.Add(new InventoryOut
                {
                    ItemId = m.ItemID,
                    Quantity = (int)m.QuantityUsed,
                    DateOut = dto.ProductionDate,
                    DestinationType = "Production",
                    UserId = userId,
                    ReferenceNo = $"PROD00{invOutCount}/{DateTime.Now.Year}"

                });

                var item = await _context.ItemMasters.FirstAsync(i => i.ItemId == m.ItemID);
                item.CurrentStock -= (int)m.QuantityUsed;
            }

            var invInCount = await _context.InventoryIns.Where(u=>u.SourceType == "Production").CountAsync();
            _context.InventoryIns.Add(new InventoryIn
            {
                ItemId = fg.ItemId,
                Quantity = (int)dto.QuantityProduced,
                DateIn = dto.ProductionDate,
                SourceType = "Production",
                UserId = userId,
                UnitCost = 0 ,
                ReferenceNo = $"PROD00{invInCount}/{DateTime.Now.Year}"
            });
            

            fg.CurrentStock += (int)dto.QuantityProduced;


            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ProductOrderDto.ProductionResultDto
            {
                ProductionId = prod.ProductionId,
                ProductionDate = prod.ProductionDate,
                ProductItemId = prod.ProductItemId,
                ProductItemName = prod.ProductItem.ItemName,
                QuantityProduced = prod.QuantityProduced
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

