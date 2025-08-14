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

            // Hitung nomor urut PO
            var poCount = await _context.PurchaseOrders.CountAsync();
            var poNumber = $"PO00{poCount + 1}/{DateTime.Now.Year}";

            var po = new PurchaseOrder
            {
                Ponumber = poNumber,
                SupplierId = dto.SupplierID,
                UserId = dto.UserID,
                Podate = dto.PODate,
                Status = "Pending",
                TotalAmount = dto.TotalAmount
            };
            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                if (item.UnitPrice <= 0)
                    throw new Exception("Price tidak bisa 0, transaksi ditolak.");

                // Cari item di master
                var existingItem = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.ItemCode == item.ItemCode);

                if (existingItem == null)
                {
                    // Buat item baru (RawMaterial)
                    var rawCount = await _context.ItemMasters
                        .Where(u => u.ItemType == "RawMaterial")
                        .CountAsync();

                    existingItem = new ItemMaster
                    {
                        ItemCode = $"RM00{rawCount + 1}/{DateTime.Now.Year}",
                        ItemName = item.ItemName,
                        ItemType = "RawMaterial",
                        Unit = item.Unit,
                        CurrentStock = item.Quantity,
                        CreatedAt = DateTime.Now,
                        MinStock = item.MinStock ?? 0,
                        Description = item.Description
                    };

                    _context.ItemMasters.Add(existingItem);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Update stok
                    existingItem.CurrentStock += item.Quantity;
                    await _context.SaveChangesAsync();
                }

                // Hitung batch number & reference untuk InventoryIn
                var invCount = await _context.InventoryIns.CountAsync();
                var batchNumber = $"BATCH{invCount + 1}";
                var invRefNo = $"PO00{poCount + 1}/{DateTime.Now.Year}";

                // Insert ke InventoryIn
                var invIn = new InventoryIn
                {
                    Quantity = item.Quantity,
                    UnitCost = item.UnitPrice,
                    DateIn = DateTime.Today,
                    ReferenceNo = invRefNo,
                    SourceType = "Purchasing",
                    UserId = dto.UserID,
                    BatchNumber = batchNumber,
                    RemainingQty = item.Quantity, // FIFO: Remaining = qty awal
                    ItemId = existingItem.ItemId
                };
                _context.InventoryIns.Add(invIn);

                // Insert detail PO
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

 



