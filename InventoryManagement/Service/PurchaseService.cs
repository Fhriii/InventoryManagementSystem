using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IPurchaseService
{
    Task<int> CreatePurchaseOrderAsync(Purchase.PurchaseOrderDto dto);
    Task<Purchase.PurchaseOrderDto> GetPurchaseOrderAsync(int purchaseOrderId);


}
public class PurchaseService : IPurchaseService
    {
        private readonly InventoryManagementContext _context;

        public PurchaseService(InventoryManagementContext context)
        {
            _context = context;
        }

        public async Task<int> CreatePurchaseOrderAsync(Purchase.PurchaseOrderDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(dto.SupplierID);
            if (supplier == null)
                throw new Exception("Supplier tidak ditemukan.");
            var user = await _context.Users.FindAsync(dto.UserID);
            if (user == null)
                throw new Exception("User tidak ditemukan.");
            var PoNumCount = await _context.PurchaseOrders.CountAsync();

            var po = new PurchaseOrder
            {
                Ponumber = $"PO00{PoNumCount}/{DateTime.Now.Year}",
                SupplierId = dto.SupplierID,
                UserId= dto.UserID,
                Podate = dto.PODate,
                Status = "Pending",
                TotalAmount = dto.TotalAmount
            };
            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            
            foreach (var item in dto.Items)
            {
                var existingItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.ItemCode == item.ItemCode);
                
                if (existingItem == null)
                {
                    var itemcount = await _context.ItemMasters.Where(u=>u.ItemType == "RawMaterial").CountAsync();

                    existingItem = new ItemMaster
                    {
                        ItemCode =  $"RM00{itemcount}/{DateTime.Now.Year}",
                        ItemName = item.ItemName,
                        ItemType = "RawMaterial",
                        Unit = item.Unit,
                        CurrentStock = item.Quantity,
           
                        CreatedAt = DateTime.Now
                    };
                    if (item.MinStock.HasValue)
                        existingItem.MinStock = item.MinStock.Value;

                    if (!string.IsNullOrEmpty(item.Description))
                        existingItem.Description = item.Description;
                    _context.ItemMasters.Add(existingItem);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    existingItem.CurrentStock += item.Quantity;
                    await _context.SaveChangesAsync();

                }

                var totalinv =await _context.InventoryIns.Where(u=>u.SourceType == "Purchasing").CountAsync(); 
                var invIn = new InventoryIn
                {
                    Quantity = item.Quantity,
                    UnitCost =  item.UnitPrice,
                    DateIn = DateTime.Today,
                    ReferenceNo = $"PO{totalinv}/{DateTime.Now.Year}",
                    SourceType = "Purchasing",
                    UserId = dto.UserID,
                    BatchNumber = $"BATCH{totalinv}",
                    RemainingQty = item.Quantity
                };
                var poDetail = new PurchaseOrderDetail()
                {
                    PurchaseOrderId = po.PurchaseOrderId,
                    ItemId= existingItem.ItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                _context.PurchaseOrderDetails.Add(poDetail);
                _context.InventoryIns.Add(invIn);
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
                UserID = purchaseOrder.UserId,
                PODate = purchaseOrder.Podate, 
                TotalAmount = purchaseOrder.TotalAmount,
                Items = purchaseOrder.PurchaseOrderDetails.Select(d => new Purchase.PurchaseOrderItemDto
                {
                    ItemCode = d.Item.ItemCode,
                    ItemName = d.Item.ItemName,
                    Unit = d.Item.Unit,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Description = d.Item.Description,
                    MinStock = d.Item.MinStock
                }).ToList()
            };

            return dto;
        }
        
    }

 



