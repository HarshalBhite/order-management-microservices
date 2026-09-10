namespace OrderService.Models;

// This class represents the "Orders" concept in our system.
// In Code First, this plain C# class is what EF Core reads to generate
// the actual database table - each property below becomes a column.
public class Order
{
    public int Id { get; set; }                  // Primary key - EF Core
                                                   // recognizes "Id" by convention
                                                   // and automatically makes it the
                                                   // primary key with auto-increment.

    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    // Navigation property - this tells EF Core "one Order has many
    // OrderItems". It's not a column itself; EF Core uses it to
    // understand and build the relationship (foreign key) between the
    // two tables.
    public List<OrderItem> Items { get; set; } = new();
}

// A simple enum to represent order status - kept intentionally small
// since business complexity isn't the point of this project.
public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled
}
