var builder = WebApplication.CreateBuilder(args);

// We will register services (DbContext, repositories, etc.) here
// step by step in the upcoming lessons - kept empty on purpose for now.

var app = builder.Build();

// A single, temporary "hello world" endpoint just to prove the project
// runs end-to-end. We will replace this with real controllers in Step 5.
app.MapGet("/", () => "OrderService is running.");

app.Run();
