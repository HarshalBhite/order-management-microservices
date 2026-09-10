using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Repositories;
using OrderService.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// This is the DI registration step we studied in Priority 2 -
// AddDbContext registers OrderDbContext with a SCOPED lifetime by
// default. "UseSqlServer" tells EF Core which database provider to use,
// and reads the actual connection string from appsettings.json below.
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Registering the repository and Unit of Work as SCOPED - matching
// OrderDbContext's lifetime. This is deliberate: since all three
// (DbContext, Repository, UnitOfWork) are Scoped, every one of them
// resolved within the same HTTP request shares the exact same
// OrderDbContext instance under the hood - which is what makes
// SaveChangesAsync() correctly commit everything together.
//
// Recall from the DI Lifetimes notes: mismatching lifetimes here (e.g.
// registering UnitOfWork as Singleton) would create a "captive
// dependency" bug - a Scoped DbContext getting trapped inside a
// long-lived object and reused incorrectly across every future request.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, OrderService.UnitOfWork.UnitOfWork>();

var app = builder.Build();

// TEMPORARY test endpoints - just to verify Repository + Unit of Work
// wiring works end-to-end. These are NOT proper REST design (no DTOs,
// no attribute routing, no status codes) - we'll replace these with a
// real OrdersController in Step 5.

app.MapPost("/test-order", async (IUnitOfWork uow) =>
{
    var order = new Order
    {
        CustomerName = "Test Customer",
        OrderDate = DateTime.UtcNow,
        Status = OrderStatus.Pending,
        TotalAmount = 250.00m,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 100.00m },
            new OrderItem { ProductName = "Gadget", Quantity = 1, UnitPrice = 50.00m }
        }
    };

    await uow.Orders.AddAsync(order);
    await uow.SaveChangesAsync(); // nothing hits the DB until this line

    return Results.Ok(order);
});

app.MapGet("/test-orders", async (IUnitOfWork uow) =>
{
    var orders = await uow.Orders.GetAllAsync();
    return Results.Ok(orders);
});

// A single, temporary "hello world" endpoint just to prove the project
// runs end-to-end. We will replace this with real controllers in Step 5.
app.MapGet("/", () => "OrderService is running.");

app.Run();
