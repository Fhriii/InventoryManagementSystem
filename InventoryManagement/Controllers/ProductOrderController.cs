using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Service;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionOrderController : ControllerBase
{
    private readonly IProductionOrderService _productionOrderService;

    public ProductionOrderController(IProductionOrderService productionOrderService)
    {
        _productionOrderService =  productionOrderService;
    }
    
    [HttpPost("{requestCode}")]
    [Authorize]
    public async Task<IActionResult> AddProduction(string requestCode)
    {
        try
        {
        
            if (requestCode == null)
                return BadRequest("Request code tidak valid.");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var prod = await _productionOrderService.CreateProductionAsync(requestCode,userId);
            return Ok(prod);
        }
        catch (Exception e)
        {
            var msg = e.InnerException != null ? e.InnerException.Message : e.Message;
            return BadRequest(msg);
        }
    }

}