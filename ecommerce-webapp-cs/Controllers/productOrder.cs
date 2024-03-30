using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_webapp_cs.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class productOrder : ControllerBase
{
	private readonly ArtsContext _context;

	public productOrder(ArtsContext context)
	{
		_context = context;
	}

	[HttpPost("cart")]
	public async Task<IActionResult> AddOrUpdateCartItem([FromBody] CartItemDto cartItem)
	{
		var cartOrder = await _context.Orders
			.FirstOrDefaultAsync(o => o.UserId == cartItem.UserId && o.Status == "InCart") ?? new Order
			{
				OrderId = GenerateOrderId(13),
				UserId = cartItem.UserId,
				OrderDate = DateTime.UtcNow,
				Status = "InCart",
				TotalPrice = 0
			};

		if (cartOrder.OrderId == null) _context.Orders.Add(cartOrder);

		var orderItem = await _context.OrderItems
			.FirstOrDefaultAsync(oi => oi.OrderId == cartOrder.OrderId && oi.ProId == cartItem.ProId);

		if (orderItem == null)
		{
			orderItem = new OrderItem
			{
				OrderItemId = GenerateOrderId(8),
				OrderId = cartOrder.OrderId,
				ProId = cartItem.ProId,
				Quantity = cartItem.Quantity,
				Price = 0 
			};
			_context.OrderItems.Add(orderItem);
		}
		else
		{
			orderItem.Quantity += cartItem.Quantity;
		}

		await _context.SaveChangesAsync();
		return Ok(new { Message = "Item added/updated in cart" });
	}

	// GET: api/Cart/{userId}
	[HttpGet("{userId}")]
	public async Task<IActionResult> ViewCart(int userId)
	{
		var cartOrder = await _context.Orders
							   .Include(o => o.OrderItems)
							   .ThenInclude(oi => oi.Pro)
							   .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "InCart");

		if (cartOrder != null)
		{
			return Ok(cartOrder);
		}

		var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == userId);
		if (hasOrders)
		{
			return NotFound("No active cart found for the user. The user has other orders.");
		}

		return NotFound("The user has no active cart or orders.");
	}

	// Convert cart to an order
	[HttpPost("place")]
	public async Task<IActionResult> PlaceOrder([FromBody] OrderCreationDto orderDto)
	{
		var order = await _context.Orders
			.Include(o => o.OrderItems)
			.FirstOrDefaultAsync(o => o.UserId == orderDto.UserId && o.Status == "InCart");

		if (order == null) return BadRequest("No cart found for the user");

		order.Status = "Placed";
		order.Note = orderDto.Note;
		order.OrderDate = DateTime.UtcNow;
		order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

		await _context.SaveChangesAsync();

		return Ok(order);
	}

	private string GenerateOrderId(int maxLength)
	{
		var random = new Random();
		var timestamp = DateTime.UtcNow.Ticks.ToString();
		var timestampLength = Math.Min(timestamp.Length, maxLength / 2);
		timestamp = timestamp.Substring(0, timestampLength);

		var remainingLength = maxLength - timestampLength;
		var randomString = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", remainingLength)
			.Select(s => s[random.Next(s.Length)]).ToArray());

		return timestamp + randomString;
	}

}
