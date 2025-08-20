using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service
{
    public interface IDeliveryService
    {
        Task<DeliveryResponseDto> CreateDelivery(int userId, string requestNumber);
        Task<dynamic> UpdateStatusDelivery(string deliveryCode);
    }

    public class DeliveryService : IDeliveryService
    {
        private readonly InventoryManagementContext _context;
        
        public DeliveryService(InventoryManagementContext context)
        {
            _context = context;
        }

        public async Task<DeliveryResponseDto> CreateDelivery(int userId, string requestNumber)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                string decodedString = System.Net.WebUtility.UrlDecode(requestNumber);
                var request = await _context.RequestOrders
                    .FirstOrDefaultAsync(u => u.RequestNumber == decodedString);
                
                if (request == null)
                    throw new Exception("Request not found");

                if (request.Status != "InProduction")
                    throw new Exception($"Request status is {request.Status}. Only 'InProduction' or 'Completed' requests can be delivered.");
                if ( request.Status == "Completed")
                {
                    throw new Exception($"RequestOrder with code {requestNumber} was Completed");

                }
                var finishedGood = await _context.ItemMasters
                    .FirstOrDefaultAsync(i => i.ItemId == request.ItemId && i.ItemType == "FinishedGoods");
                
                if (finishedGood == null)
                    throw new Exception("Finished good tidak ditemukan");

                var availableStock = await _context.Inventories
                    .Where(i => i.ItemId == request.ItemId && i.Quantity > 0)
                    .SumAsync(i => i.Quantity) ?? 0;

                Console.WriteLine($"DEBUG - FG ItemId: {request.ItemId}, Available: {availableStock}, Needed: {request.Quantity}");

                if (availableStock < request.Quantity)
                    throw new Exception($"Stok finished goods tidak cukup. Tersedia: {availableStock}, Butuh: {request.Quantity}");

                int deliveryCount = await _context.Deliveries.CountAsync();
                var newDelivery = new Delivery
                {
                    DeliveryDate = DateOnly.FromDateTime(DateTime.Today),
                    RequestId = request.RequestId,
                    UserId = userId,
                    DeliveryStatus = "Pending",
                    DeliveryNumber = $"DEL00{deliveryCount}/{DateTime.Now.Year}"
                };
                _context.Deliveries.Add(newDelivery);
                await _context.SaveChangesAsync(); 

                decimal qtyToDeliver = request.Quantity;
                
                var fifoStocks = await _context.Inventories
                    .Where(i => i.ItemId == request.ItemId && i.Quantity > 0) 
                    .OrderBy(i => i.DateIn)
                    .ThenBy(i => i.InventoryId)
                    .ToListAsync();

                foreach (var stock in fifoStocks)
                {
                    if (qtyToDeliver <= 0) break;
                    
                    decimal takeQty = Math.Min((decimal)stock.Quantity, qtyToDeliver);

                    Console.WriteLine($"DEBUG - Taking {takeQty} from InventoryId: {stock.InventoryId}, Remaining stock: {stock.Quantity - takeQty}");

                    var newInvOut = new InventoryOut
                    {
                        UserId = userId,
                        DateOut = DateTime.Now,
                        DestinationType = "Delivery",
                        QuantityUsed = takeQty,
                        ReferenceNo = newDelivery.DeliveryNumber,
                        InventoryId = stock.InventoryId,
                        UnitCost = stock.UnitCost ?? 0
                    };
                    _context.InventoryOuts.Add(newInvOut);

                    stock.Quantity -= takeQty;
                    stock.UpdateAt = DateTime.Now;
                    qtyToDeliver -= takeQty;
                }

                if (qtyToDeliver > 0)
                    throw new Exception($"Stock finished goods tidak cukup. Sisa yang belum terpenuhi: {qtyToDeliver}");

                request.Status = "Completed";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DeliveryResponseDto
                {
                    DeliveryNumber = newDelivery.DeliveryNumber,
                    DeliveryDate = newDelivery.DeliveryDate,
                    DeliveryStatus = newDelivery.DeliveryStatus,
                    RequestNumber = request.RequestNumber
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<dynamic> UpdateStatusDelivery(string deliveryCode)
        {
            string deliveerycode = System.Net.WebUtility.UrlDecode(deliveryCode);
            var deliveryOrder = await _context.Deliveries.FirstOrDefaultAsync(u => u.DeliveryNumber == deliveerycode);
            if (deliveryOrder == null)
            {
                throw new Exception($"Delivery with code {deliveerycode} not found");
            }

            try
            {
                deliveryOrder.DeliveryStatus = "Delivered";
                await _context.SaveChangesAsync();
                return deliveryOrder.DeliveryStatus;
                
            }catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}