using Microsoft.AspNetCore.Mvc;
using InventoryService.DTOs;
using InventoryService.Models;
using InventoryService.UnitOfWork;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IUnitOfWork unitOfWork, ILogger<InventoryController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET api/inventory
    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAllItems()
    {
        var items = await _unitOfWork.InventoryItems.GetAllAsync();
        return Ok(items.Select(MapToDto).ToList());
    }

    // GET api/inventory/5
    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryItemDto>> GetItemById(int id)
    {
        var item = await _unitOfWork.InventoryItems.GetByIdAsync(id);

        if (item is null)
        {
            _logger.LogWarning("Inventory item {ItemId} was requested but does not exist", id);
            return NotFound();
        }

        return Ok(MapToDto(item));
    }

    // POST api/inventory
    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> CreateItem(CreateInventoryItemDto dto)
    {
        var item = new InventoryItem
        {
            ProductName = dto.ProductName,
            QuantityAvailable = dto.QuantityAvailable,
            QuantityReserved = 0
        };

        await _unitOfWork.InventoryItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Inventory item {ItemId} created for product {ProductName} with quantity {QuantityAvailable}",
            item.Id, item.ProductName, item.QuantityAvailable);

        return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, MapToDto(item));
    }

    private static InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto
        {
            Id = item.Id,
            ProductName = item.ProductName,
            QuantityAvailable = item.QuantityAvailable,
            QuantityReserved = item.QuantityReserved
        };
    }
}
