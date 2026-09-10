using OrderService.Repositories;

namespace OrderService.UnitOfWork;

// This is the "bundling + atomic commit" contract from the Priority 2
// notes. Right now we only have ONE repository (Orders), so the
// transactional benefit isn't dramatic yet - but this becomes genuinely
// important once we add more repositories/entities later (e.g. if a
// business operation ever needs to update both an Order AND some other
// entity together, SaveChangesAsync() here ensures both succeed or both
// fail as one unit, exactly like the multi-repository example in the
// notes).
public interface IUnitOfWork
{
    IOrderRepository Orders { get; }

    // Returns the number of rows affected - a standard EF Core
    // SaveChanges return value, useful for logging/diagnostics later.
    Task<int> SaveChangesAsync();
}
