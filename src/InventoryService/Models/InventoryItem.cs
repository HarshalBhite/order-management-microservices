namespace InventoryService.Models;

// A simple domain model - one row per product, tracking how many units
// are in stock. Deliberately simple: this service's whole job is
// answering "is there enough stock?" and "reserve some stock" - it
// doesn't need to know anything about orders, customers, or pricing.
// That separation of concerns IS the service boundary in action.
public class InventoryItem
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int QuantityAvailable { get; set; }

    // Row version, used later (Step 13/14) to help handle concurrent
    // reservation attempts safely - harmless to have now, not used yet.
    public int QuantityReserved { get; set; }
}
