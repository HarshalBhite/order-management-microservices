namespace OrderService.DTOs;

// Generic wrapper so ANY list endpoint (not just orders) can return this
// same shape - matches the paginated response example from the
// Priority 3 REST API notes (data + totalCount + page + pageSize +
// totalPages), so clients always know how many pages exist without a
// separate API call.
public class PagedResultDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
