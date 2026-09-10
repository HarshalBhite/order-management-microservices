using Microsoft.EntityFrameworkCore;
using OrderService.Data;

var builder = WebApplication.CreateBuilder(args);

// This is the DI registration step we studied in Priority 2 -
// AddDbContext registers OrderDbContext with a SCOPED lifetime by
// default. "UseSqlServer" tells EF Core which database provider to use,
// and reads the actual connection string from appsettings.json below.
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

var app = builder.Build();

// A single, temporary "hello world" endpoint just to prove the project
// runs end-to-end. We will replace this with real controllers in Step 5.
app.MapGet("/", () => "OrderService is running.");

app.Run();
