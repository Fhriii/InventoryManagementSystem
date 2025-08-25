using Microsoft.EntityFrameworkCore;
using WebApplication1.Dto;
using WebApplication1.Models;
using WebApplication1.Service;
using Xunit;

namespace WebApplication1.Tests.Service;

public class ItemMasterServiceTests
{
    // Helper method untuk membuat DbContext in-memory yang bersih untuk setiap tes
    private InventoryManagementContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryManagementContext>()
            // Gunakan nama database yang unik untuk setiap tes agar tidak ada konflik
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new InventoryManagementContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    // ===================================
    // Tes untuk metode GetItemMaster
    // ===================================

    [Fact]
    public async Task GetItemMaster_ShouldReturnAllItems()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.ItemMasters.AddRange(
            new ItemMaster { ItemCode = "A001", ItemName = "Item 1", Unit = "PCS" },
            new ItemMaster { ItemCode = "A002", ItemName = "Item 2", Unit = "BOX" }
        );
        await context.SaveChangesAsync();

        var service = new ItemMasterService(context);

        // Act
        var result = await service.GetItemMaster();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("A001", result[0].ItemCode);
    }

    [Fact]
    public async Task GetItemMaster_ShouldAppendWarning_WhenStockIsBelowMinimum()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.ItemMasters.Add(
            new ItemMaster { ItemCode = "LOW01", ItemName = "Low Stock Item", Unit = "PCS", Description = "Initial Description." }
        );
        await context.SaveChangesAsync();

        var service = new ItemMasterService(context);

        // Act
        var result = await service.GetItemMaster();

        // Assert
        Assert.Single(result); // Memastikan hanya ada 1 item
        Assert.EndsWith(" (Stock below minimum)", result[0].Description);
        Assert.StartsWith("Initial Description.", result[0].Description);
    }

    // ===================================
    // Tes untuk metode AddItem
    // ===================================

    [Fact]
    public async Task AddItem_ShouldAddNewItem_WhenDataIsValid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new ItemMasterService(context);
        var newItemDto = new ItemMasterDto
        {
            ItemCode = "B001",
            ItemName = "New Item",
            Unit = "PCS",
            Description = "A brand new item"
        };

        // Act
        var result = await service.AddItem(newItemDto);
        var itemInDb = await context.ItemMasters.FindAsync("B001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newItemDto.ItemCode, result.ItemCode);
        Assert.Equal(newItemDto.ItemName, result.ItemName);

        Assert.NotNull(itemInDb); // Memastikan item benar-benar tersimpan di database
        Assert.Equal("B001", itemInDb.ItemCode);
    }

    [Fact]
    public async Task AddItem_ShouldThrowInvalidOperationException_WhenItemCodeExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.ItemMasters.Add(new ItemMaster { ItemCode = "C001", ItemName = "Existing Item", Unit = "PCS" });
        await context.SaveChangesAsync();

        var service = new ItemMasterService(context);
        var duplicateItemDto = new ItemMasterDto { ItemCode = "C001", ItemName = "Duplicate Item", Unit = "BOX" };

        // Act & Assert
        // Memverifikasi bahwa exception dengan tipe yang benar dilempar
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddItem(duplicateItemDto));
    }

    [Theory]
    [InlineData(null, "Some Name", "PCS", "ItemCode is required.")] // ItemCode null
    [InlineData(" ", "Some Name", "PCS", "ItemCode is required.")]  // ItemCode whitespace
    [InlineData("C002", null, "PCS", "ItemName is required.")]      // ItemName null
    [InlineData("C002", " ", "PCS", "ItemName is required.")]       // ItemName whitespace
    public async Task AddItem_ShouldThrowArgumentException_ForMissingRequiredFields(string itemCode, string itemName, string unit, string expectedMessage)
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new ItemMasterService(context);
        var invalidDto = new ItemMasterDto
        {
            ItemCode = itemCode,
            ItemName = itemName,
            Unit = unit
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.AddItem(invalidDto));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task AddItem_ShouldThrowArgumentException_WhenMinStockIsGreaterThanCurrentStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new ItemMasterService(context);
        var dto = new ItemMasterDto
        {
            ItemCode = "D001",
            ItemName = "Test Item",
            Unit = "PCS",
         
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddItem(dto));
    }
}