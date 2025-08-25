using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Service;

public interface IItemMasterService
{
    Task<List<ItemMasterDto>> GetItemMaster();
    Task<ItemMasterDto> AddItem(ItemMasterDto dto);

}

public class ItemMasterService : IItemMasterService
{
    private readonly InventoryManagementContext _context;

    public ItemMasterService(InventoryManagementContext context)
    {
        _context = context;
    }
    public async Task<List<ItemMasterDto>> GetItemMaster()
    {
        var itemMasterList = await _context.ItemMasters
            .Select(u => new ItemMasterDto
            {
                ItemCode = u.ItemCode,
                ItemName = u.ItemName,
                ItemType = u.ItemType,
                Description = u.Description,
           
                Unit = u.Unit
            })
            .ToListAsync();

        return itemMasterList;
    }
    public async Task<ItemMasterDto> AddItem(ItemMasterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemCode))
            throw new ArgumentException("ItemCode is required.");

        if (string.IsNullOrWhiteSpace(dto.ItemName))
            throw new ArgumentException("ItemName is required.");

        if (string.IsNullOrWhiteSpace(dto.Unit))
            throw new ArgumentException("Unit is required.");

        var exists = await _context.ItemMasters
            .AnyAsync(i => i.ItemCode == dto.ItemCode);
        if (exists)
            throw new InvalidOperationException($"Item with code '{dto.ItemCode}' already exists.");
        
        // Save new item
        var newItem = new ItemMaster
        {
            ItemCode = dto.ItemCode.Trim(),
            ItemName = dto.ItemName.Trim(),
            Description = dto.Description,
            ItemType = dto.ItemType,
            Unit = dto.Unit.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ItemMasters.Add(newItem);
        await _context.SaveChangesAsync();

        return new ItemMasterDto
        {
            ItemCode = newItem.ItemCode,
            ItemName = newItem.ItemName, 
            Description = newItem.Description,
            ItemType = newItem.ItemType,
            Unit = newItem.Unit
        };
    }



    

}