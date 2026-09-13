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

    public async Task<(List<Order> Orders, int TotalCount)> GetOrdersAsync(
        OrderStatus? status,
        string? sortBy,
        string sortDirection,
        int page,
        int pageSize)
    {
        // IQueryable, not IEnumerable - this is important. At this
        // point, NO SQL has run yet - we're building up the query
        // step by step, and EF Core only translates it to actual SQL
        // and executes it once we call .ToListAsync() at the very end.
        // This is what lets .Where()/.OrderBy()/.Skip().Take() all get
        // pushed down into ONE efficient SQL query, instead of pulling
        // everything into memory first.
        IQueryable<Order> query = _context.Orders.Include(o => o.Items);

        // FILTERING - only applied if the caller actually supplied a
        // status (mirrors the "optional query parameter" pattern from
        // the REST notes: skip the filter entirely if not provided).
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        // SORTING - a small whitelist of allowed sort columns, rather
        // than accepting any arbitrary string. This is a real, common
        // practice: letting a client sort by literally any column name
        // via raw string matching can open up unintended behavior, so
        // we explicitly map known values only.
        bool descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLower() switch
        {
            "totalamount" => descending
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "customername" => descending
                ? query.OrderByDescending(o => o.CustomerName)
                : query.OrderBy(o => o.CustomerName),
            // Default sort column if none specified, or an unrecognized
            // value is passed - OrderDate, since it's the most natural
            // default for a list of orders.
            _ => descending
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate)
        };

        // Total count of MATCHING rows (after filtering, before paging)
        // - needed so the client can calculate TotalPages. Important:
        // this must be counted BEFORE .Skip()/.Take() are applied,
        // otherwise it would only count the current page's rows.
        var totalCount = await query.CountAsync();

        // PAGINATION - translated by EF Core into SQL's OFFSET/FETCH
        // (SQL Server's equivalent of Skip/Take), so the database only
        // ever returns the one page of rows actually needed - not the
        // entire table.
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }
}
