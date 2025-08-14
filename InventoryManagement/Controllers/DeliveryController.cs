using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Service;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveryController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }
    [HttpPost("{requestNumber}")]
    [Authorize]
    public async Task<ActionResult> AddDelivery(string requestNumber)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var deliver = await _deliveryService.CreateDelivery(userId, requestNumber);
            return Ok(deliver);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }   
}