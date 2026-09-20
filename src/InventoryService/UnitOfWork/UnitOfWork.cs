using InventoryService.Data;
using InventoryService.Repositories;

namespace InventoryService.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly InventoryDbContext _context;

    public IInventoryRepository InventoryItems { get; }

    public UnitOfWork(InventoryDbContext context, IInventoryRepository inventoryRepository)
    {
        _context = context;
        InventoryItems = inventoryRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
