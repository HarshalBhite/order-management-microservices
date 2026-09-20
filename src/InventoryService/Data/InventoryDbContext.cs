using Microsoft.EntityFrameworkCore;
using InventoryService.Models;

namespace InventoryService.Data;

// A completely SEPARATE DbContext from OrderService's OrderDbContext -
// pointing at its own, independent database (InventoryServiceDb, set up
// in appsettings.json). This is the actual microservices principle in
// action from the Priority 2 notes: each service owns its own data
// exclusively. OrderService will NEVER query this database directly -
// any interaction has to go through InventoryService's API (Step 13).
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
}
