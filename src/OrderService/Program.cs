using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Repositories;
using OrderService.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// This is the DI registration step we studied in Priority 2 -
// AddDbContext registers OrderDbContext with a SCOPED lifetime by
// default. "UseSqlServer" tells EF Core which database provider to use,
// and reads the actual connection string from appsettings.json below.
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

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

// Registers MVC-style controllers (like OrdersController) with the DI
// container and routing system - required for attribute routing
// ([Route], [HttpGet], etc.) to work at all.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Fixes the second open item from Step 3: serializes enums
        // (like OrderStatus) as their STRING name ("Pending") instead
        // of the raw underlying integer (0). Applied globally here, so
        // every controller/DTO in the app benefits automatically,
        // without repeating this per-property.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// NOTE: the previous IgnoreCycles band-aid from Step 3 has been REMOVED
// here on purpose. It's no longer needed - OrderDto/OrderItemDto are
// flat, purpose-built classes with no back-reference to their parent,
// so there is no circular reference for the serializer to ever
// encounter in the first place. This is the "real fix" mentioned back
// in Step 3.

var app = builder.Build();

// Wires up attribute-routed controllers (OrdersController) to actually
// handle incoming requests. Without this line, [Route]/[HttpGet]/etc.
// attributes would be defined but never actually reachable.
app.MapControllers();

app.Run();
