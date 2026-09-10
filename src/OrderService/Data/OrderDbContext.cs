using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

// DbContext is EF Core's central class - it represents a session with
// the database and is what actually knows how to translate our C#
// classes into SQL, and back.
//
// Recall from Priority 2 notes: this DbContext is what will be
// registered with a SCOPED lifetime later (in Program.cs) - meaning
// one instance per HTTP request, which is also what lets it act like a
// Unit of Work once we get there in Step 3.
public class OrderDbContext : DbContext
{
    // This constructor accepts "options" (like the connection string)
    // from outside - that's what allows Dependency Injection to control
    // how this DbContext is configured, instead of hardcoding it here.
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    // Each DbSet<T> represents one table. EF Core uses these to know
    // which classes should become tables when we run a migration.
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Optional but good practice: fine-tune how EF Core maps things,
    // beyond what it can figure out purely by convention.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensures TotalAmount and UnitPrice are stored with proper
        // decimal precision in SQL Server (decimal(18,2) = up to 18
        // total digits, 2 after the decimal point) - without this, EF
        // Core defaults to a precision that can silently truncate
        // money values, a real, common bug in financial-style systems.
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");
    }
}
