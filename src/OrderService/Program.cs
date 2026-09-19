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

// STEP 7 - Swagger/OpenAPI registration.
// AddEndpointsApiExplorer() lets ASP.NET Core discover our minimal/
// attribute-routed endpoints so Swagger can describe them.
// AddSwaggerGen() is what actually builds the OpenAPI document by
// reading our controllers, action methods, and DTOs via reflection -
// this is why the docs stay automatically in sync with the real code,
// as discussed in the Priority 3 REST API notes.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Learning project API for Order Management - demonstrates Repository, Unit of Work, DTOs, and REST design."
    });
});

var app = builder.Build();

// Swagger is deliberately gated behind IsDevelopment() - a real,
// security-conscious practice. Exposing your full API surface
// (endpoints, request/response shapes) publicly in production isn't
// something you want by default; Swagger here is a development/testing
// convenience, not something to ship to a real production environment
// unprotected.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    });
}

// Wires up attribute-routed controllers (OrdersController) to actually
// handle incoming requests. Without this line, [Route]/[HttpGet]/etc.
// attributes would be defined but never actually reachable.
app.MapControllers();

app.Run();
