using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IProductionOrderService
{
    Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(string requestCode, int userId);
}
public class ProductionService : IProductionOrderService
{
    private readonly InventoryManagementContext _context;

    public ProductionService(InventoryManagementContext context)
    {
        _context = context;
    }

    public async Task<ProductOrderDto.ProductionResultDto> CreateProductionAsync(string requestCode, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            string requestNumber = System.Net.WebUtility.UrlDecode(requestCode);
            var requestOrder = await _context.RequestOrders.FirstOrDefaultAsync(u => u.RequestNumber == requestNumber);
            if (requestOrder == null)
            {
                throw new Exception($"RequestOrder with code {requestNumber} not found");
            }

            int itemid = requestOrder.ItemId;
            var fg = await _context.ItemMasters.Include(u => u.Inventories)
                .FirstOrDefaultAsync(i => i.ItemId == itemid && i.ItemType == "FinishedGoods");
            if (fg == null) throw new Exception("Finished good tidak ditemukan.");

            var materials = await _context.BillOfMaterials
                .Where(b => b.FinishedGoodId == itemid)
                .Select(b => new ProductOrderDto.MaterialUsageDto
                {
                    ItemID = b.RawMaterialId,
                    QuantityUsed = b.QuantityRequired * requestOrder.Quantity
                })
                .ToListAsync();

            if (!materials.Any())
                throw new Exception("BOM untuk produk ini belum ada.");

            foreach (var m in materials)
            {
                var available = await _context.Inventories
                    .Where(x => x.ItemId == m.ItemID && (x.Quantity ?? 0) > 0)
                    .SumAsync(x => x.Quantity) ?? 0;

                var itemName = await _context.ItemMasters
                    .Where(i => i.ItemId == m.ItemID)
                    .Select(i => i.ItemName)
                    .FirstOrDefaultAsync() ?? m.ItemID.ToString();

                if (available < (int)m.QuantityUsed)
                    throw new Exception($"Stok tidak cukup untuk item {itemName}. Butuh {(int)m.QuantityUsed}, tersedia {available}.");
            }

            var prodCount = await _context.Productions.CountAsync();
            var prodRef = $"PROD00{prodCount + 1}/{DateTime.Now.Year}";
            var currentTime = DateTime.Now;

            var prod = new Production
            {
                ProductionDate = currentTime,
                ProductItemId = itemid,
                QuantityProduced = requestOrder.Quantity,
                UserId = userId,
                ReferenceNo = prodRef
            };
            _context.Productions.Add(prod);
            requestOrder.Status = "InProduction";
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

                var qtyToTake = (int)m.QuantityUsed;
                var fifoBatches = await _context.Inventories
                    .Where(b => b.ItemId == m.ItemID && (b.Quantity ?? 0) > 0)
                    .OrderBy(b => b.DateIn)
                    .ThenBy(b => b.InventoryId)
                    .ToListAsync();

                decimal materialCostForThisItem = 0m;

                foreach (var batch in fifoBatches)
                {
                    if (qtyToTake <= 0) break;
                    var available = batch.Quantity ?? 0;
                    if (available <= 0) continue;

                    var take = Math.Min(available, qtyToTake);

                    batch.Quantity = available - take;
                    batch.UpdateAt = currentTime;

                    materialCostForThisItem += (batch.UnitCost ?? 0) * take;
                    qtyToTake -= take;
                    
                }
                

                if (qtyToTake > 0)
                    throw new Exception($"FIFO error: stok tidak cukup untuk ItemID {m.ItemID}");

                totalMaterialCost += materialCostForThisItem;

                var invOut = new InventoryOut
                {
                    QuantityUsed = (int)m.QuantityUsed,
                    DateOut = currentTime,
                    DestinationType = "Production",
                    UserId = userId,
                    ReferenceNo = prodRef,
                    UnitCost = materialCostForThisItem / (int)m.QuantityUsed
                };
                _context.InventoryOuts.Add(invOut);
            }

            var producedQty = requestOrder.Quantity;
            var fgUnitCost = producedQty > 0 ? Math.Round(totalMaterialCost / producedQty, 2) : 0m;

            var fgIn = new InventoryIn
            {
                ItemId = fg.ItemId,
                Quantity = producedQty,
                UnitCost = fgUnitCost,
                CreatedAt = currentTime,
                SourceType = "Production",
                ReferenceNo = prodRef,
                UserId = userId,
                TotalAmount = fgUnitCost * producedQty
            };
            _context.InventoryIns.Add(fgIn);

            var existingFgInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ItemId == fg.ItemId);

            if (existingFgInventory != null)
            {
                existingFgInventory.Quantity = (existingFgInventory.Quantity ?? 0) + producedQty;
                existingFgInventory.UpdateAt = currentTime;
            }
            else
            {
                var fgInventory = new Inventory
                {
                    ItemId = fg.ItemId,
                    Quantity = producedQty,
                    UnitCost = fgUnitCost,
                    BatchNumber = $"BATCH-{fg.ItemCode}-{DateTime.Now:yyyyMMddHHmmss}",
                    DateIn = currentTime,
                    UpdateAt = currentTime
                };
                _context.Inventories.Add(fgInventory);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ProductOrderDto.ProductionResultDto
            {
                ProductionCode = prod.ReferenceNo,
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