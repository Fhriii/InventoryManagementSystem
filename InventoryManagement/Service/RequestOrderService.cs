using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IRequestOrderService
{
    Task<dynamic> CreateRequestAsync(RequestOrders.RequestDto dto, int userId);
    Task<dynamic> CreateRequestExistAsync(RequestOrders.RequestOrderItemExist dto, int userId);
    Task<dynamic> UpdateStatusRequest(string requestCode);
}

public class RequestOrderService : IRequestOrderService
{
    private readonly InventoryManagementContext _context;

    public RequestOrderService(InventoryManagementContext context)
    {
        _context = context;
    }

    public async Task<dynamic> CreateRequestAsync(RequestOrders.RequestDto dto, int userId)
    {
        string statusStockItem = "";

        var reqcount = await _context.RequestOrders.CountAsync();
        var request = new RequestOrder
        {
            RequestNumber = $"REQ00{reqcount}/{DateTime.Now.Year}",
            UserId = userId,
            RequestDate = DateTime.UtcNow,
            Status = "Pending",
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            Quantity = dto.Quantity
        };
        _context.RequestOrders.Add(request);
        await _context.SaveChangesAsync();

        var itemcount = await _context.ItemMasters.Where(u => u.ItemType == "FinishedGoods").CountAsync();
        var fgItem = new ItemMaster
        {
            ItemCode = $"FG00{itemcount}/{DateTime.Now.Year}",
            ItemName = dto.FinishedGoodName,
            Unit = dto.FinishedGoodUnit,
            ItemType = "FinishedGoods",
            CreatedAt = DateTime.Now
        };
        _context.ItemMasters.Add(fgItem);
        await _context.SaveChangesAsync();

        request.ItemId = fgItem.ItemId;
        await _context.SaveChangesAsync();

        foreach (var raw in dto.Items)
        {
            var rawItem = await _context.ItemMasters
                .FirstOrDefaultAsync(i => i.ItemCode == raw.ItemCode && i.ItemName == raw.ItemName);

            if (rawItem == null)
            {
                rawItem = new ItemMaster
                {
                    ItemCode = raw.ItemCode,
                    ItemName = raw.ItemName,
                    Unit = raw.Unit,
                    ItemType = "RawMaterial",
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

            var rawStock = await _context.Inventories
                .Where(i => i.ItemId == rawItem.ItemId)
                .SumAsync(i => i.Quantity);

            statusStockItem = rawStock < raw.Quantity
                ? $"Stok {rawItem.ItemName} tidak cukup. Tersedia: {rawStock}, Butuh: {raw.Quantity}"
                : "Stok Cukup";

            _context.RequestOrderDetails.Add(new RequestOrderDetail
            {
                RequestId = request.RequestId,
                ItemId = rawItem.ItemId,
                Quantity = raw.Quantity
            });
        }

        await _context.SaveChangesAsync();
        return new
        {
            message = "Request Order Created",
            statusStock = statusStockItem,
            requestNo = request.RequestNumber,
        };
    }

    public async Task<dynamic> CreateRequestExistAsync(RequestOrders.RequestOrderItemExist dto, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            decimal weightedAvgPrice;
            decimal totalUnitCost = 0;
            List<string> stockMessages = new();
            decimal? rawStock = 0;
            decimal totalNeeded = 0;
            
            var itemMaster = await _context.ItemMasters.FirstOrDefaultAsync(u => u.ItemCode == dto.ItemCode);
            if (itemMaster == null) throw new Exception("Item tidak ditemukan.");

            var reqcount = await _context.RequestOrders.CountAsync();
            var request = new RequestOrder
            {
                RequestNumber = $"REQ00{reqcount}/{DateTime.Now.Year}",
                UserId = userId,
                RequestDate = DateTime.UtcNow,
                Status = "Pending",
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                ItemId = itemMaster.ItemId,
                Quantity = dto.Quantity
            };
            
            _context.RequestOrders.Add(request);
            await _context.SaveChangesAsync(); 

            var bomList = await _context.BillOfMaterials
                .Where(b => b.FinishedGoodId == itemMaster.ItemId)
                .ToListAsync();

            if (!bomList.Any())
                throw new Exception("BOM untuk produk ini tidak ditemukan.");

            var requestOrderDetails = new List<RequestOrderDetail>();

            foreach (var bom in bomList)
            {
                totalNeeded = bom.QuantityRequired * dto.Quantity;

                rawStock = await _context.Inventories
                    .Where(i => i.ItemId == bom.RawMaterialId)
                    .SumAsync(i => i.Quantity);

                if (rawStock < totalNeeded)
                {
                    var errorMsg = $"Stok raw material {bom.RawMaterialId} tidak cukup. Tersedia: {rawStock}, Butuh: {totalNeeded}";
                    stockMessages.Add(errorMsg);
                    throw new Exception(errorMsg);
                }

                var invData = await _context.Inventories
                    .Where(i => i.ItemId == bom.RawMaterialId && i.Quantity > 0)
                    .ToListAsync();

                if (!invData.Any())
                {
                    stockMessages.Add($"Tidak ada stok untuk raw material ID {bom.RawMaterialId}");
                    continue;
                }

                var totalCost = invData.Sum(x => x.Quantity * x.UnitCost);
                var totalQty = invData.Sum(x => x.Quantity);

                weightedAvgPrice = (decimal)(totalCost / totalQty);
                totalUnitCost += bom.QuantityRequired * weightedAvgPrice;

                requestOrderDetails.Add(new RequestOrderDetail
                {
                    RequestId = request.RequestId,
                    ItemId = bom.RawMaterialId,
                    Quantity = totalNeeded,
                    UnitPrice = weightedAvgPrice
                });
            }

            if (requestOrderDetails.Any())
            {
                _context.RequestOrderDetails.AddRange(requestOrderDetails);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new
            {
                message = "Request Order Created",
                statusStock = stockMessages.Any() 
                    ? string.Join("; ", stockMessages) 
                    : $"Stok Cukup, Tersedia: {rawStock}, Butuh: {totalNeeded}",
                requestNo = request.RequestNumber,
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<dynamic> UpdateStatusRequest(string requestCode)
    {
        string requestNumber = System.Net.WebUtility.UrlDecode(requestCode);
        var requestOrder = await _context.RequestOrders.FirstOrDefaultAsync(u => u.RequestNumber == requestNumber);
        if (requestOrder == null)
        {
            throw new Exception($"RequestOrder with code {requestNumber} not found");
        }

        try
        {
            requestOrder.Status = "Canceled";
            await _context.SaveChangesAsync();
            return requestOrder.Status;
        }catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
