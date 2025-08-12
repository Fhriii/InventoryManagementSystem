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
                CurrentStock = u.CurrentStock,
                MinStock = u.MinStock,
                Unit = u.Unit
            })
            .ToListAsync();

        foreach (var item in itemMasterList)
        {
            if (item.CurrentStock < item.MinStock)
            {
                item.Description += " (Stock below minimum)";
            }
        }

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

        if (dto.CurrentStock < 0)
            throw new ArgumentOutOfRangeException(nameof(dto.CurrentStock), "CurrentStock cannot be negative.");

        if (dto.MinStock < 0)
            throw new ArgumentOutOfRangeException(nameof(dto.MinStock), "MinStock cannot be negative.");

        if (dto.MinStock > dto.CurrentStock)
            throw new ArgumentException("MinStock cannot be greater than CurrentStock.");

        // Save new item
        var newItem = new ItemMaster
        {
            ItemCode = dto.ItemCode.Trim(),
            ItemName = dto.ItemName.Trim(),
            CurrentStock = dto.CurrentStock,
            Description = dto.Description,
            ItemType = dto.ItemType,
            MinStock = dto.MinStock,
            Unit = dto.Unit.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ItemMasters.Add(newItem);
        await _context.SaveChangesAsync();

        return new ItemMasterDto
        {
            ItemCode = newItem.ItemCode,
            ItemName = newItem.ItemName,
            CurrentStock = newItem.CurrentStock,
            Description = newItem.Description,
            ItemType = newItem.ItemType,
            MinStock = newItem.MinStock,
            Unit = newItem.Unit
        };
    }



    

}