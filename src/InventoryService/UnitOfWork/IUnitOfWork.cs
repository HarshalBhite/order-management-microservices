using InventoryService.Repositories;

namespace InventoryService.UnitOfWork;

public interface IUnitOfWork
{
    IInventoryRepository InventoryItems { get; }
    Task<int> SaveChangesAsync();
}
