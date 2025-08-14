using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Service;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestOrderController : ControllerBase
{
    private readonly IRequestOrderService _requestOrderService;

    public RequestOrderController(IRequestOrderService requestOrderService)
    {
        _requestOrderService = requestOrderService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddRequest(RequestOrders.RequestDto request)
    {
        try
        {
            // === Validasi umum ===
            if (string.IsNullOrWhiteSpace(request.RequestNumber))
                return BadRequest("RequestNumber wajib diisi.");
            
            // === Mode BOM ===
            if (request.ItemCode != null)
            {
                if (request.Quantity == null || request.Quantity <= 0)
                    return BadRequest("Quantity wajib diisi jika menggunakan FinishedGoodID.");

                if (request.Items != null && request.Items.Any())
                    return BadRequest("Items tidak boleh diisi jika menggunakan FinishedGoodID.");
            }
            // === Mode manual ===
            else
            {
                if (request.Items == null || !request.Items.Any())
                    return BadRequest("Items wajib diisi jika tidak menggunakan ItemCode.");

                foreach (var item in request.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemCode))
                        return BadRequest("ItemCode wajib diisi.");
                    if (item.Quantity <= 0)
                        return BadRequest($"Quantity untuk item {item.ItemCode ?? "(tanpa kode)"} harus lebih dari 0.");
           
                }
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var req = await _requestOrderService.CreateRequestAsync(request,userId);
            return Ok(new
            {
                message = "Add Request Successfuly",
                RequestId = req
            });
        }
        catch (Exception e)
        {
            var errorMessage = e.Message;
            if (e.InnerException != null)
                errorMessage += " | Inner: " + e.InnerException.Message;

            return BadRequest(errorMessage);        }
    }

}