using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.UnitOfWork;

namespace OrderService.Controllers;

// [ApiController] enables automatic 400 Bad Request responses on model
// validation failure, and smarter [FromBody]/[FromRoute] inference -
// covered in the Web API & Routing notes.
//
// [Route("api/[controller]")] - [controller] is a placeholder token that
// becomes "Orders" (this class name minus "Controller"), lowercased by
// convention -> base route becomes "api/orders".
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrdersController> _logger;

    // ILogger<OrdersController> is injected by DI, same mechanism as
    // everything else we've registered. The generic <OrdersController>
    // parameter automatically tags every log entry from this class with
    // its source ("OrderService.Controllers.OrdersController"), which is
    // itself another structured property useful for filtering later.
    public OrdersController(IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET api/orders?status=Pending&sortBy=totalAmount&sortDirection=desc&page=1&pageSize=10
    //
    // All parameters here are OPTIONAL (nullable, or have default
    // values) - ASP.NET Core automatically binds query string values to
    // these parameters by NAME (the "automatic model binding" from the
    // Web API & Routing notes) - no manual URL parsing needed.
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAllOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] string? sortBy,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // Basic guardrails - never trust client-supplied paging values
        // blindly. Without this, a client could request pageSize=999999
        // and effectively defeat the whole point of pagination.
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (orders, totalCount) = await _unitOfWork.Orders.GetOrdersAsync(
            status, sortBy, sortDirection, page, pageSize);

        var result = new PagedResultDto<OrderDto>
        {
            Data = orders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result); // 200 OK
    }

    // GET api/orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);

        if (order is null)
        {
            // LogWarning, not LogError - a missing order isn't a system
            // failure, just a normal "not found" case. {OrderId} here is
            // a NAMED structured property (not string interpolation) -
            // this is what makes it filterable/searchable later, exactly
            // as discussed at the top of this step.
            _logger.LogWarning("Order {OrderId} was requested but does not exist", id);

            // Correct REST status code for "resource doesn't exist" -
            // from the Priority 3 status code notes. NOT 200 with an
            // empty body, NOT 500.
            return NotFound();
        }

        return Ok(MapToDto(order));
    }

    // POST api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        // Server calculates TotalAmount itself from the items - never
        // trusts a client-supplied total, since that would let a client
        // claim any amount it wants for a "total".
        var order = new Order
        {
            CustomerName = createOrderDto.CustomerName,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = createOrderDto.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // LogInformation with structured properties - {OrderId},
        // {CustomerName}, and {TotalAmount} are each stored as separate
        // fields, not just flattened into the message text. This is
        // exactly the kind of log entry that becomes genuinely useful
        // once Correlation IDs are added later (Step 15) - you'll be
        // able to trace this exact order's creation across every
        // service it touches.
        _logger.LogInformation(
            "Order {OrderId} created for customer {CustomerName} with total {TotalAmount}",
            order.Id, order.CustomerName, order.TotalAmount);

        // 201 Created - the correct status code for a successful POST
        // that created a new resource (from Priority 3 notes). CreatedAtAction
        // also sets the response's "Location" header to point at
        // GET api/orders/{id} for this new order - a REST convention
        // that tells the client exactly where to fetch/find what it
        // just created.
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.Id },
            MapToDto(order));
    }

    // Private helper - keeps the entity -> DTO mapping logic in one
    // place rather than repeating it in every action method above.
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
