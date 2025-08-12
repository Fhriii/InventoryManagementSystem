using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;
using WebApplication1.Service;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemMasterController : ControllerBase
{
    private readonly IItemMasterService _itemMasterService;

    public ItemMasterController(IItemMasterService itemMasterService)
    {
        _itemMasterService = itemMasterService;
    }
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetItem()
    {
        try
        {
            var items = await _itemMasterService.GetItemMaster();

            if (items == null || !items.Any())
            {
                return NotFound();
            }

            return Ok(items);

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }
        
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddItemMaster(ItemMasterDto dto)
    {
        try
        {
            var item = await _itemMasterService.AddItem(dto);
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) // duplikat atau kondisi bisnis gagal
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex) // error lain yang tak terduga
        {
            return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
        }
    }
}