using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Models;

namespace InventoryService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _context.InventoryItems.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems.ToListAsync();
    }

    public async Task AddAsync(InventoryItem item)
    {
        await _context.InventoryItems.AddAsync(item);
    }
}
