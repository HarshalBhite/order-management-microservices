namespace OrderService.Models;

// Represents one line item within an Order - e.g. "2 units of Product A".
// This is the "many" side of the one-to-many relationship with Order.
public class OrderItem
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // Foreign key property - by naming convention (OrderId matches the
    // Order class name + "Id"), EF Core automatically understands this
    // links back to the Order table, without us writing any extra
    // configuration.
    public int OrderId { get; set; }

    // Navigation property back to the parent Order - lets our C# code
    // do things like orderItem.Order.CustomerName if ever needed.
    public Order? Order { get; set; }
}
