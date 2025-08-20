using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IPurchaseService
{
    Task<int> CreatePurchaseOrderAsync(Purchase.PurchaseOrderDto dto,int userid);
    Task<int> CreatePurchaseOrderItemExistAsync(Purchase.PurchaseOrderExist dto,int userid);
    Task<Purchase.PurchaseOrderDto> GetPurchaseOrderAsync(int purchaseOrderId);


}
public class PurchaseService : IPurchaseService
{
    private readonly InventoryManagementContext _context;

    public PurchaseService(InventoryManagementContext context)
    {
        _context = context;
    }

    public async Task<int> CreatePurchaseOrderAsync(Purchase.PurchaseOrderDto dto,int userid)
    {
        var supplier = await _context.Suppliers.FindAsync(dto.SupplierID);
        if (supplier == null)
            throw new Exception("Supplier tidak ditemukan.");

        var user = await _context.Users.FindAsync(userid);
        if (user == null)
            throw new Exception("User tidak ditemukan.");

        var poCount = await _context.PurchaseOrders.CountAsync();
        var poNumber = $"PO00{poCount + 1}/{DateTime.Now.Year}";
        var totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

        var po = new PurchaseOrder
        {
            Ponumber = poNumber,
            SupplierId = dto.SupplierID,
            UserId = userid,
            Podate = dto.PODate ?? DateOnly.FromDateTime(DateTime.Now),
            Status = "Pending",
            TotalAmount = totalAmount
        };
        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();

        foreach (var item in dto.Items)
        {
            if (item.UnitPrice <= 0)
                throw new Exception("Price tidak bisa 0, transaksi ditolak.");

       
                var rawCount = await _context.ItemMasters
                    .Where(u => u.ItemType == "RawMaterial")
                    .CountAsync();

                var newItem = new ItemMaster
                {
                    ItemCode = $"RM00{rawCount + 1}",
                    ItemName = item.ItemName,
                    ItemType = "RawMaterial",
                    Unit = item.Unit,
                    CreatedAt = DateTime.Now,
                    Description = item.Description
                };

                _context.ItemMasters.Add(newItem);
                await _context.SaveChangesAsync();
            

            var invCount = await _context.Inventories.CountAsync();
            var batchNumber = $"BATCH{invCount + 1}";
            var invRefNo = $"PO00{poCount + 1}/{DateTime.Now.Year}";

            var inventory = new Inventory
            {
                Quantity = item.Quantity,
                UnitCost = item.UnitPrice,
                DateIn = DateTime.Now,
                BatchNumber = batchNumber,
                ItemId = newItem.ItemId,
                UpdateAt = DateTime.Now
            };
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            var invIn = new InventoryIn
            {
                ReferenceNo = invRefNo,
                SourceType = "Purchasing",
                UserId = userid,
                ItemId = newItem.ItemId,
                UnitCost = item.UnitPrice,
                CreatedAt = DateTime.Now,
                TotalAmount = totalAmount,
                Quantity = item.Quantity
            };
            
            _context.InventoryIns.Add(invIn);
   
            var poDetail = new PurchaseOrderDetail
            {
                PurchaseOrderId = po.PurchaseOrderId,
                ItemId = newItem.ItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
            _context.PurchaseOrderDetails.Add(poDetail);
        }

        await _context.SaveChangesAsync();
        return po.PurchaseOrderId;
    }

    public async Task<int> CreatePurchaseOrderItemExistAsync(Purchase.PurchaseOrderExist dto,int userid)
    {
         var supplier = await _context.Suppliers.FindAsync(dto.SupplierID);
        if (supplier == null)
            throw new Exception("Supplier tidak ditemukan.");

        var user = await _context.Users.FindAsync(userid);
        if (user == null)
            throw new Exception("User tidak ditemukan.");

        var poCount = await _context.PurchaseOrders.CountAsync();
        var poNumber = $"PO00{poCount + 1}/{DateTime.Now.Year}";
        var totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

        var po = new PurchaseOrder
        {
            Ponumber = poNumber,
            SupplierId = dto.SupplierID,
            UserId = userid,
            Podate = dto.PODate ?? DateOnly.FromDateTime(DateTime.Now),
            Status = "Pending",
            TotalAmount = totalAmount
        };
        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new Exception("Quantity tidak bisa 0, transaksi ditolak.");

            var existingItem = await _context.ItemMasters
                .FirstOrDefaultAsync(x => x.ItemCode == item.ItemCode);
            await _context.SaveChangesAsync();
        

            var invCount = await _context.Inventories.CountAsync();
            var batchNumber = $"BATCH{invCount + 1}";
            var invPoCount = await _context.InventoryIns.Where(u => u.SourceType == "Purchasing").CountAsync();
            var invRefNo = $"PO00{invPoCount+1}/{DateTime.Now.Year}";
          
            var invIn = new InventoryIn
            {
                ItemId = existingItem.ItemId,
                ReferenceNo = invRefNo,
                SourceType = "Purchasing",
                UserId = userid,
                CreatedAt = DateTime.Now,
                UnitCost = item.UnitPrice,
                TotalAmount = item.UnitPrice * item.Quantity,
                Quantity = item.Quantity
                
            };
            _context.InventoryIns.Add(invIn);
            var inventory = new Inventory
            {
                Quantity = item.Quantity,
                UnitCost = item.UnitPrice,
                DateIn = DateTime.Now,
                BatchNumber = batchNumber,
                ItemId = existingItem.ItemId,
                UpdateAt = DateTime.Now
            };
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            var poDetail = new PurchaseOrderDetail
            {
                PurchaseOrderId = po.PurchaseOrderId,
                ItemId = existingItem.ItemId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
            _context.PurchaseOrderDetails.Add(poDetail);
        }

        await _context.SaveChangesAsync();
        return po.PurchaseOrderId;
    }

    public async Task<Purchase.PurchaseOrderDto> GetPurchaseOrderAsync(int purchaseOrderId)
    {
        var purchaseOrder = _context.PurchaseOrders.Include(u=>u.PurchaseOrderDetails).ThenInclude(u=>u.Item).FirstOrDefault(u=>u.PurchaseOrderId == purchaseOrderId);
        if (purchaseOrder == null)
        {
            throw new Exception("Purchase Order tidak ditemukan.");
        }

        var dto = new Purchase.PurchaseOrderDto
        {
            SupplierID = purchaseOrder.SupplierId,
            PODate = purchaseOrder.Podate, 
            Items = purchaseOrder.PurchaseOrderDetails.Select(d => new Purchase.PurchaseOrderItemDto
            {
                ItemName = d.Item.ItemName,
                Unit = d.Item.Unit,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                Description = d.Item.Description,
            }).ToList()
        };

        return dto;
    }
    
}

 



