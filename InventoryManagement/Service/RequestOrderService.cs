using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IRequestOrderService
{
    Task<int> CreateRequestAsync(RequestOrders.RequestDto dto);
}

public class RequestOrderService : IRequestOrderService
{
    private readonly InventoryManagementContext _context;

    public RequestOrderService(InventoryManagementContext context)
    {
        _context = context;
    }
    public async Task<int> CreateRequestAsync(RequestOrders.RequestDto dto)
    {
        var reqcount = await _context.RequestOrders.CountAsync();
        var request = new RequestOrder
        {
            RequestNumber = $"REQ00{reqcount}/{DateTime.Now.Year}",
            UserId = dto.UserID,
            RequestDate = dto.RequestDate,
            Status = "Pending",
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone
            
            
        };
        _context.RequestOrders.Add(request);
        await _context.SaveChangesAsync();

        if (dto.FinishedGoodID.HasValue)
        {
            var bomList = await _context.BillOfMaterials
                .Where(b => b.FinishedGoodId == dto.FinishedGoodID.Value)
                .ToListAsync();

            if (!bomList.Any())
                throw new Exception("BOM untuk produk ini tidak ditemukan.");

            foreach (var bom in bomList)
            {
                var totalNeeded = bom.QuantityRequired * dto.Quantity;
                var item = await _context.ItemMasters.FindAsync(bom.RawMaterialId);
                if (item == null)
                    throw new Exception($"Raw material ID {bom.RawMaterialId} tidak ditemukan.");

                if (item.CurrentStock < totalNeeded)
                    throw new Exception($"Stok {item.ItemName} tidak cukup.");

                item.CurrentStock -= (int)totalNeeded;

                _context.RequestOrderDetails.Add(new RequestOrderDetail
                {
                    RequestId = request.RequestId,
                    ItemId = bom.RawMaterialId,
                    Quantity = (int)totalNeeded,
                    UnitPrice = 100
                    
                });
            }
        }
        else
        {
            var fgItem = await _context.ItemMasters
                .FirstOrDefaultAsync(i => i.ItemCode == dto.FinishedGoodCode);

            if (fgItem == null)
            {
                var itemcount = await _context.ItemMasters.Where(u => u.ItemType == "FinishedGoods").CountAsync();
                fgItem = new ItemMaster
                {
                    ItemCode = $"FG00{itemcount}/{DateTime.Now.Year}",
                    ItemName = dto.FinishedGoodName,
                    Unit = dto.FinishedGoodUnit,
                    ItemType = "FinishedGoods", 
                    CurrentStock = 0,
                    CreatedAt = DateTime.Now
                };
                _context.ItemMasters.Add(fgItem);
                await _context.SaveChangesAsync();
            }

            foreach (var raw in dto.Items)
            {
                var rawItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(i => i.ItemCode == raw.ItemCode);

                if (rawItem == null)
                {
                    rawItem = new ItemMaster
                    {
                        ItemCode = raw.ItemCode,
                        ItemName = raw.ItemName,
                        Unit = raw.Unit,
                        ItemType = "RawMaterial", 
                        CurrentStock = 0,
                        CreatedAt = DateTime.Now
                    };
                    _context.ItemMasters.Add(rawItem);
                    await _context.SaveChangesAsync();
                }

                _context.BillOfMaterials.Add(new BillOfMaterial
                {
                    FinishedGoodId = fgItem.ItemId,
                    RawMaterialId = rawItem.ItemId,
                    QuantityRequired = raw.Quantity,
                    Unit = raw.Unit
                });

                if (rawItem.CurrentStock < raw.Quantity)
                    throw new Exception($"Stok {rawItem.ItemName} tidak cukup.");

                rawItem.CurrentStock -= (int)raw.Quantity;

                _context.RequestOrderDetails.Add(new RequestOrderDetail
                {
                    RequestId = request.RequestId,
                    ItemId = rawItem.ItemId,
                    Quantity = (int)raw.Quantity
                });
            }
        }


        await _context.SaveChangesAsync();
        return request.RequestId;
    }

}