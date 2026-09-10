using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

// The concrete implementation - this is the ONLY class in the whole
// project that directly talks to OrderDbContext for Order data. Every
// other layer (business logic, controllers) goes through
// IOrderRepository instead.
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    // The OrderDbContext is injected here by the DI container - we never
    // write "new OrderDbContext(...)" ourselves. Because both this
    // repository and the DbContext will be registered as Scoped, every
    // repository used within the same HTTP request shares the exact
    // same DbContext instance.
    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        // Include(o => o.Items) tells EF Core to also load the related
        // OrderItems in the same query (a JOIN under the hood), instead
        // of leaving Items empty. Without this, Items would silently
        // come back as an empty list even if the order has items.
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        // Note: this only STAGES the new order with the DbContext's
        // change tracker - it does NOT hit the database yet. Nothing is
        // actually saved until SaveChangesAsync() is called via the
        // Unit of Work. This is exactly the "batch changes together"
        // behavior discussed in the Unit of Work notes.
        await _context.Orders.AddAsync(order);
    }
}
