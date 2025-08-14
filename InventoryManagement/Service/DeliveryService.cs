using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service
{
    public interface IDeliveryService
    {
        Task<DeliveryResponseDto> CreateDelivery(int userId, string requestNumber);
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
            string decodedString = System.Net.WebUtility.UrlDecode(requestNumber);

            var request = await _context.RequestOrders
                .FirstOrDefaultAsync(u => u.RequestNumber == decodedString);
            if (request == null)
                throw new Exception("Request not found");

            int invOutCount = await _context.InventoryOuts
                .Where(u => u.DestinationType == "Delivery")
                .CountAsync();
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

            var requestItems = await _context.RequestOrderDetails
                .Where(u => u.RequestId == request.RequestId)
                .ToListAsync();

            foreach (var reqItem in requestItems)
            {
                int qtyNeeded = reqItem.Quantity;

                var fifoStocks = await _context.InventoryIns
                    .Where(i => i.ItemId == reqItem.ItemId && i.RemainingQty > 0)
                    .OrderBy(i => i.DateIn)
                    .ThenBy(i => i.InventoryInId)
                    .ToListAsync();

                foreach (var stock in fifoStocks)
                {
                    if (qtyNeeded <= 0)
                        break;

                    int takeQty = Math.Min((int)stock.RemainingQty, qtyNeeded);

                    var newInvOut = new InventoryOut
                    {
                        UserId = userId,
                        DateOut = DateTime.Now,
                        DestinationType = "Delivery",
                        ItemId = reqItem.ItemId,
                        Quantity = takeQty,
                        ReferenceNo = $"DEL00{invOutCount}/{DateTime.Now.Year}",
                    };

                    _context.InventoryOuts.Add(newInvOut);

                    stock.RemainingQty -= takeQty;
                    qtyNeeded -= takeQty;

                    var itemMaster = await _context.ItemMasters
                        .FirstAsync(i => i.ItemId == reqItem.ItemId);
                    itemMaster.CurrentStock -= takeQty;

                    invOutCount++;
                }

                if (qtyNeeded > 0)
                    throw new Exception($"Stock tidak cukup untuk item ID {reqItem.ItemId}");
            }

            request.Status = "Completed";

            await _context.SaveChangesAsync();

            return new DeliveryResponseDto
            {
                DeliveryNumber = newDelivery.DeliveryNumber,
                DeliveryDate = newDelivery.DeliveryDate,
                DeliveryStatus = newDelivery.DeliveryStatus,
                RequestNumber = request.RequestNumber
            };
        }
    }
}
