using OrderService.Models;

namespace OrderService.DTOs;

// This is the "flat, purpose-built response class" from the Priority 3
// notes. Notice: NO navigation property back to Order here at all - this
// is what genuinely fixes the circular reference issue from Step 3
// (rather than just hiding it with IgnoreCycles). There's simply nothing
// here for the serializer to loop on.
public class OrderItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }

    // Returned as a STRING (e.g. "Pending"), not the raw enum integer -
    // fixes the second open item from Step 3. We'll configure this
    // globally via JsonStringEnumConverter in Program.cs, so this
    // property doesn't need any special code itself.
    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
