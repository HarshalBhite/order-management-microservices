using OrderService.Data;
using OrderService.Repositories;

namespace OrderService.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;

    // Exposes the Orders repository as a property - this is the
    // "repositories wrapped as properties in one interface-class
    // combination" idea from the notes.
    public IOrderRepository Orders { get; }

    public UnitOfWork(OrderDbContext context, IOrderRepository orderRepository)
    {
        _context = context;
        Orders = orderRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        // This single call commits EVERYTHING that has been staged
        // across ALL repositories sharing this same DbContext instance,
        // in one database transaction - all succeed together, or all
        // fail together. This is the actual point of Unit of Work,
        // exactly as covered in the Priority 2 notes.
        return await _context.SaveChangesAsync();
    }
}
