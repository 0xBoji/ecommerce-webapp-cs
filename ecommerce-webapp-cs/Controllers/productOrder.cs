using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Authorization;
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
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var cartOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.UserId == cartItem.UserId && o.Status == "InCart");

                if (cartOrder == null)
                {
                    cartOrder = new Order
                    {
                        OrderId = GenerateOrderId(13),
                        UserId = cartItem.UserId,
                        OrderDate = DateTime.UtcNow,
                        Status = "InCart",
                        TotalPrice = 0
                    };
                    _context.Orders.Add(cartOrder);
                    await _context.SaveChangesAsync();
                }


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
                        Price = (await _context.Products.FirstOrDefaultAsync(p => p.ProId == cartItem.ProId))?.Price ?? 0
                    };
                    _context.OrderItems.Add(orderItem);
                }
                else
                {
                    orderItem.Quantity += cartItem.Quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = "Item added/updated in cart" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // log the exception
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
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

    // convert cart to an order
    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderCreationDto orderDto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == orderDto.UserId && o.Status == "InCart");

        if (order == null) return BadRequest("No cart found for the user.");

        // Validate and apply the voucher
        if (!string.IsNullOrEmpty(orderDto.VoucherCode))
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == orderDto.VoucherCode && v.StartDate <= DateTime.UtcNow && v.Expired >= DateTime.UtcNow);

            if (voucher == null)
            {
                return BadRequest("Invalid or expired voucher code.");
            }

            order.TotalPrice -= voucher.Amount; // adjust total price based on the voucher
            order.VoucherId = voucher.VoucherId; // associate voucher with the order
        }

        order.Status = "Placed";
        order.Note = orderDto.Note;
        order.OrderDate = DateTime.UtcNow;

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

    [HttpPost("processPayment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentDto paymentDto)
    {
        // fetch the order
        var order = await _context.Orders
                                  .FirstOrDefaultAsync(o => o.OrderId == paymentDto.OrderId && o.UserId == paymentDto.UserId);

        if (order == null)
        {
            return BadRequest("Order not found.");
        }

        // process the payment through the payment gateway
        // this is a simplified example; in a real application, you would call the payment gateway's API here
        var paymentProcessed = ProcessPaymentThroughGateway(paymentDto);

        if (!paymentProcessed)
        {
            return BadRequest("Payment failed.");
        }

        // update the order status to "Paid" or similar
        order.Status = "Paid";
        await _context.SaveChangesAsync();

        var paymentRecord = new Payment
        {
            OrderId = order.OrderId,
            PaymentType = "CreditCard",
            Status = "Completed",
            Amount = order.TotalPrice,
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(paymentRecord);
        await _context.SaveChangesAsync();

        return Ok("Payment processed successfully.");
    }

    [HttpPost("request-refund")]
    public async Task<IActionResult> RequestRefund([FromBody] RefundRequestDto refundRequestDto)
    {
        // check if the order exists and belongs to the user
        var order = await _context.Orders.FindAsync(refundRequestDto.OrderId);
        if (order == null || order.UserId != refundRequestDto.UserId)
        {
            return BadRequest("Order not found or does not belong to the user.");
        }

        // create a new return request
        var returnRequest = new ReturnRequest
        {
            OrderId = refundRequestDto.OrderId,
            Reason = refundRequestDto.Reason,
            RequestStatus = "Pending",
            RequestDate = DateTime.UtcNow
        };

        _context.ReturnRequests.Add(returnRequest);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Refund request submitted successfully." });
    }




    private bool ProcessPaymentThroughGateway(PaymentDto paymentDto)
    {
        // simulate payment processing
        // in a real application, you would integrate with a payment gateway here
        return true; // Assume payment is always successful for demonstration purposes
    }


}