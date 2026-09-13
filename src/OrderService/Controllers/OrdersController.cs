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

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET api/orders
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();

        // Mapping entities -> DTOs, right here in the controller for
        // now (fine at this project's scale; larger real systems often
        // use a mapping library like AutoMapper to avoid writing this
        // by hand everywhere - worth knowing that exists, not required
        // here).
        var dtos = orders.Select(MapToDto).ToList();

        return Ok(dtos); // 200 OK
    }

    // GET api/orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);

        if (order is null)
        {
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
