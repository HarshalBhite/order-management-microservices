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

    // New for Step 6: filtering, sorting, and pagination, all applied at
    // the DATABASE level (translated to SQL by EF Core), not fetched
    // into memory first. Returns both the page of data AND the total
    // matching count (needed to calculate TotalPages in the response).
    //
    // "status" and "sortBy" are optional (nullable) - if not supplied,
    // no filter/default sort is applied, matching the "optional query
    // parameter" pattern from the REST API notes.
    Task<(List<Order> Orders, int TotalCount)> GetOrdersAsync(
        OrderStatus? status,
        string? sortBy,
        string sortDirection,
        int page,
        int pageSize);

    // Note: no explicit "Update" method here. With EF Core, once an
    // entity is fetched via GetByIdAsync, it's already being "tracked"
    // by the DbContext - so changing its properties and later calling
    // SaveChangesAsync() (via Unit of Work) is enough. This is a common,
    // real EF Core pattern - explicit Update() methods are only needed
    // in more specific scenarios (e.g. updating an entity that was never
    // fetched in this request).
}
