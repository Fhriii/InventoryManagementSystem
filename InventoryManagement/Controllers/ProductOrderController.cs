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
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProduction([FromBody] ProductOrderDto.ProductionDto dto)
    {
        try
        {
            if (dto == null)
                return BadRequest("Data produksi tidak boleh kosong.");

            if (dto.FinishedGoodID <= 0)
                return BadRequest("ItemId harus diisi.");

            if (dto.QuantityProduced <= 0)
                return BadRequest("QuantityProduced harus lebih dari 0.");

            if (dto.ProductionDate == default)
                return BadRequest("Tanggal produksi tidak valid.");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var prod = await _productionOrderService.CreateProductionAsync(dto,userId);
            return Ok(prod);
        }
        catch (Exception e)
        {
            var msg = e.InnerException != null ? e.InnerException.Message : e.Message;
            return BadRequest(msg);
        }
    }

}