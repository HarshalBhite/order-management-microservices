using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Repositories;
using InventoryService.UnitOfWork;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

// Points at its OWN, separate database (InventoryServiceDb) - this
// service knows nothing about OrderServiceDb, and never will. This is
// the actual service boundary from the Priority 2 Microservices notes,
// now real: two services, two databases, no shared data access.
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IUnitOfWork, InventoryService.UnitOfWork.UnitOfWork>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory Service API",
        Version = "v1",
        Description = "Learning project API for Inventory Management - a genuine second microservice with its own database."
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Service API v1");
    });
}

app.MapControllers();

app.Run();
