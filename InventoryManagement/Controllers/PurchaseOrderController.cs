using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Service;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]

public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseOrderController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] Purchase.PurchaseOrderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var poId = await _purchaseService.CreatePurchaseOrderAsync(dto);
            return Ok(new 
            { 
                Message = "Purchase Order berhasil dibuat.", 
                PurchaseOrderID = poId 
            });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new 
            { 
                error = ex.Message, 
                inner = GetFullErrorText(ex) 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = ex.Message, 
                inner = ex.InnerException?.Message 
            });
        }
    }

    private string GetFullErrorText(Exception ex)
    {
        var message = ex.Message;
        var inner = ex.InnerException;
        while (inner != null)
        {
            message += " --> " + inner.Message;
            inner = inner.InnerException;
        }
        return message;
    }

    [HttpGet("{purchaseOrderId}")]
    [Authorize]
    public async Task<IActionResult> GetPurchaseOrders(int purchaseOrderId)
    {
        try
        {
            var data = await _purchaseService.GetPurchaseOrderAsync(purchaseOrderId);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                error = ex.Message, 
                inner = ex.InnerException?.Message 
            });
        }
        
    }
    
}