using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static List<Order> _orders = new()
    {
        new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            ShippingAddress = "123 Main St, City",
            TotalAmount = 1299.98m,
            OrderDate = DateTime.Now.AddDays(-2),
            Status = "Delivered",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, ProductName = "Laptop", Quantity = 1, UnitPrice = 999.99m },
                new OrderItem { ProductId = 3, ProductName = "Headphones", Quantity = 1, UnitPrice = 199.99m }
            }
        },
        new Order
        {
            Id = 2,
            CustomerName = "Jane Smith",
            ShippingAddress = "456 Oak Ave, Town",
            TotalAmount = 699.99m,
            OrderDate = DateTime.Now.AddDays(-1),
            Status = "Processing",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 2, ProductName = "Smartphone", Quantity = 1, UnitPrice = 699.99m }
            }
        }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetOrders()
    {
        return Ok(_orders);
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost]
    public ActionResult<Order> CreateOrder(Order order)
    {
        order.Id = _orders.Max(o => o.Id) + 1;
        order.OrderDate = DateTime.Now;
        order.Status = "Pending";
        _orders.Add(order);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateOrder(int id, Order order)
    {
        var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        existingOrder.CustomerName = order.CustomerName;
        existingOrder.ShippingAddress = order.ShippingAddress;
        existingOrder.TotalAmount = order.TotalAmount;
        existingOrder.Status = order.Status;
        existingOrder.Items = order.Items;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        _orders.Remove(order);
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateOrderStatus(int id, [FromBody] string status)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;
        return NoContent();
    }
} 