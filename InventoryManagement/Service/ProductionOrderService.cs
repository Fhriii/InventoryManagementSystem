using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IProductionOrderService
{
    Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(ProductOrderDto.ProductionDto dto, int userId);
}

public class ProductionService : IProductionOrderService
{
    private readonly InventoryManagementContext _context;

    public ProductionService(InventoryManagementContext context)
    {
        _context = context;
    }

    public async Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(ProductOrderDto.ProductionDto dto, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var itemid = await _context.ItemMasters.FirstOrDefaultAsync(u => u.ItemCode == dto.FinishedGoodCode);
            var fg = await _context.ItemMasters
                .FirstOrDefaultAsync(i => i.ItemId == itemid.ItemId && i.ItemType == "FinishedGoods");
            if (fg == null) throw new Exception("Finished good tidak ditemukan.");

            List<ProductOrderDto.MaterialUsageDto> materials;
            if (dto.Materials == null || !dto.Materials.Any())
            {
                materials = await _context.BillOfMaterials
                    .Where(b => b.FinishedGoodId == itemid.ItemId)
                    .Select(b => new ProductOrderDto.MaterialUsageDto
                    {
                        ItemID = b.RawMaterialId,
                        QuantityUsed = b.QuantityRequired * dto.QuantityProduced
                    })
                    .ToListAsync();

                if (!materials.Any())
                    throw new Exception("BOM untuk produk ini belum ada.");
            }
            else
            {
                materials = dto.Materials;
            }

            foreach (var m in materials)
            {
                var available = await _context.InventoryIns
                    .Where(x => x.ItemId == m.ItemID && (x.RemainingQty ?? 0) > 0)
                    .SumAsync(x => (int?)x.RemainingQty) ?? 0;

                var itemName = await _context.ItemMasters
                    .Where(i => i.ItemId == m.ItemID)
                    .Select(i => i.ItemName)
                    .FirstOrDefaultAsync() ?? m.ItemID.ToString();

                if (available < (int)m.QuantityUsed)
                    throw new Exception($"Stok batch (FIFO) tidak cukup untuk item {itemName}. Butuh {(int)m.QuantityUsed}, tersedia {available}.");
            }

            var prodCount = await _context.Productions.CountAsync();
            var prodRef = $"PROD00{prodCount}/{DateTime.Now.Year}";

            var prod = new Production
            {
                ProductionDate = dto.ProductionDate?? DateTime.UtcNow,
                ProductItemId = itemid.ItemId,
                QuantityProduced = (int)dto.QuantityProduced,
                UserId = userId,
                ReferenceNo = prodRef
            };
            _context.Productions.Add(prod);
            await _context.SaveChangesAsync();

            decimal totalMaterialCost = 0m;

            foreach (var m in materials)
            {
                _context.ProductionMaterialUsages.Add(new ProductionMaterialUsage
                {
                    ProductionId = prod.ProductionId,
                    RawMaterialItemId = m.ItemID,
                    QuantityUsed = (int)m.QuantityUsed
                });

                var invOut = new InventoryOut
                {
                    ItemId = m.ItemID,
                    Quantity = (int)m.QuantityUsed,
                    DateOut = dto.ProductionDate ?? DateTime.UtcNow,
                    DestinationType = "Production",
                    UserId = userId,
                    ReferenceNo = prodRef
                };
                _context.InventoryOuts.Add(invOut);
                await _context.SaveChangesAsync();
                
                
                var qtyToTake = (int)m.QuantityUsed;
                var fifoBatches = await _context.InventoryIns
                    .Where(b => b.ItemId == m.ItemID && (b.RemainingQty ?? 0) > 0)
                    .OrderBy(b => b.DateIn)
                    .ThenBy(b => b.InventoryInId)
                    .ToListAsync();

                foreach (var batch in fifoBatches)
                {
                    if (qtyToTake <= 0) break;
                    var available = batch.RemainingQty ?? 0;
                    if (available <= 0) continue;

                    var take = Math.Min(available, qtyToTake);

                    _context.InventoryOutDetails.Add(new InventoryOutDetail
                    {
                        InventoryOutId = invOut.InventoryOutId,
                        InventoryInId = batch.InventoryInId,
                        QuantityUsed = take,
                        UnitCost = batch.UnitCost
                    });

                    batch.RemainingQty = available - take;

                    totalMaterialCost += (batch.UnitCost * take);

                    qtyToTake -= take;
                }

                if (qtyToTake > 0)
                    throw new Exception($"FIFO error: stok batch untuk ItemID {m.ItemID} berubah saat proses.");

                var item = await _context.ItemMasters.FirstAsync(i => i.ItemId == m.ItemID);
                item.CurrentStock -= (int)m.QuantityUsed;
            }

            var producedQty = (int)dto.QuantityProduced;
            var fgUnitCost = producedQty > 0 ? Math.Round(totalMaterialCost / producedQty, 2) : 0m;

            var invInCount = await _context.InventoryIns.Where(u => u.SourceType == "Production").CountAsync();
            var fgBatchNo = $"BATCH-{fg.ItemCode}-{DateTime.Now:yyyyMMddHHmmss}";

            var fgIn = new InventoryIn
            {
                ItemId = fg.ItemId,
                Quantity = producedQty,
                UnitCost = fgUnitCost,
                DateIn = dto.ProductionDate ?? DateTime.UtcNow,
                SourceType = "Production",
                ReferenceNo = prodRef,          
                UserId = userId,
                BatchNumber = fgBatchNo,
                RemainingQty = producedQty
            };
            _context.InventoryIns.Add(fgIn);

            fg.CurrentStock += producedQty;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ProductOrderDto.ProductionResultDto
            {
                ProductionId = prod.ProductionId,
                ProductionDate = prod.ProductionDate,
                ProductItemId = prod.ProductItemId,
                ProductItemName = fg.ItemName,
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
