namespace InventoryService.DTOs;

public class InventoryItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
}

// Used later (Step 13) when OrderService calls InventoryService to
// check/reserve stock before confirming an order - not wired up to any
// endpoint yet, just defining the shape now while we're in this file.
public class CreateInventoryItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
}
