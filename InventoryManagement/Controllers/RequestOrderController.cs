using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> AddRequest(RequestOrders.RequestDto request)
    {
        try
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

            return BadRequest(errorMessage);
            
        }
    }

    [HttpPost("ItemExist")]
    [Authorize]
    public async Task<IActionResult> AddRequestExistItem([FromBody] RequestOrders.RequestOrderItemExist dto)
    {
        try
        {
          
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var req = await _requestOrderService.CreateRequestExistAsync(dto,userId);
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

            return BadRequest(errorMessage);
            
        }
    }
}