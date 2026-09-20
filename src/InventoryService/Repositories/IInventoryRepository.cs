using InventoryService.Models;

namespace InventoryService.Repositories;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<List<InventoryItem>> GetAllAsync();
    Task AddAsync(InventoryItem item);
}
