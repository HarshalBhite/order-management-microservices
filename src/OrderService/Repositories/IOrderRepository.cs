using OrderService.Models;

namespace OrderService.Repositories;

// This interface is the actual "Repository Pattern" contract we studied
// in Priority 2. Business logic (and later, controllers) will depend on
// THIS interface, never on OrderRepository directly - that's what makes
// it possible to swap the implementation (e.g. with an in-memory fake)
// for unit testing, without touching business logic code.
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> GetAllAsync();
    Task AddAsync(Order order);

    // Note: no explicit "Update" method here. With EF Core, once an
    // entity is fetched via GetByIdAsync, it's already being "tracked"
    // by the DbContext - so changing its properties and later calling
    // SaveChangesAsync() (via Unit of Work) is enough. This is a common,
    // real EF Core pattern - explicit Update() methods are only needed
    // in more specific scenarios (e.g. updating an entity that was never
    // fetched in this request).
}
