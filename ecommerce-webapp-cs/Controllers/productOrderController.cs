using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class productOrderController : ControllerBase
    {
        private readonly ArtsContext _context;

        public productOrderController(ArtsContext context)
        {
            _context = context;
        }

        [HttpPost("cart")]
        public async Task<IActionResult> AddOrUpdateCartItem([FromBody] CartItemDto cartItem)
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var cartOrder = await _context.Orders
                        .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "InCart");

                    if (cartOrder == null)
                    {
                        cartOrder = new Order
                        {
                            OrderId = GenerateOrderId(13),
                            UserId = userId,
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
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }
        }


        [HttpGet("cart")]
        public async Task<IActionResult> ViewCart()
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var cartOrder = await _context.Orders
                           .Include(o => o.OrderItems)
                           .ThenInclude(oi => oi.Pro)
                           .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "InCart");

            if (cartOrder != null)
            {
                return Ok(cartOrder);
            }

            return NotFound("The user has no active cart.");
        }


        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderCreationDto orderDto)
        {
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "InCart");

            if (order == null) return BadRequest("No cart found for the user.");

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
            var order = await _context.Orders
                                      .FirstOrDefaultAsync(o => o.OrderId == paymentDto.OrderId && o.UserId == paymentDto.UserId);

            if (order == null)
            {
                return BadRequest("Order not found.");
            }

            var paymentProcessed = ProcessPaymentThroughGateway(paymentDto);

            if (!paymentProcessed)
            {
                return BadRequest("Payment failed.");
            }

            order.Status = "Paid";
            await _context.SaveChangesAsync();

            var paymentRecord = new Payment
            {
                OrderId = order.OrderId,
                PaymentType = paymentDto.PaymentType,
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
            var order = await _context.Orders.FindAsync(refundRequestDto.OrderId);
            if (order == null || order.UserId != refundRequestDto.UserId)
            {
                return BadRequest("Order not found or does not belong to the user.");
            }

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
            return true;
        }

        [HttpPost("negotiate")]
        public async Task<IActionResult> NegotiatePrice([FromBody] NegotiationDto negotiationDto)
        {
            // Retrieve UserId from the session
            var userIdString = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int UserId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            // Use UserId from the session instead of the request body for further operations
            var acceptedNegotiation = await _context.Negotiations
                .FirstOrDefaultAsync(n => n.ProId == negotiationDto.ProId && n.UserId == UserId && n.Status == "Accepted");

            if (acceptedNegotiation != null)
            {
                return BadRequest("Negotiation for this product has already been accepted and cannot be modified.");
            }

            var existingNegotiation = await _context.Negotiations
                .FirstOrDefaultAsync(n => n.ProId == negotiationDto.ProId && n.UserId == UserId);

            if (existingNegotiation != null)
            {
                existingNegotiation.NegotiatedPrice = negotiationDto.NegotiatedPrice;
                existingNegotiation.Status = "Pending";
                _context.Negotiations.Update(existingNegotiation);
            }
            else
            {
                var newNegotiation = new Negotiation
                {
                    ProId = negotiationDto.ProId,
                    UserId = UserId, // Use the UserId from session
                    NegotiatedPrice = negotiationDto.NegotiatedPrice,
                    Status = "Pending"
                };
                _context.Negotiations.Add(newNegotiation);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Negotiation submitted successfully." });
        }



        [HttpGet("getnegotiation/{negotiationId}")]
        public async Task<IActionResult> GetNegotiation(int negotiationId)
        {
            var negotiation = await _context.Negotiations.FindAsync(negotiationId);
            if (negotiation == null)
            {
                return NotFound();
            }

            return Ok(negotiation);
        }

        [HttpGet("user-negotiations/{userId}")]
        public async Task<IActionResult> GetNegotiationsByUserId(int userId)
        {
            var userNegotiations = await _context.Negotiations
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!userNegotiations.Any())
            {
                return NotFound($"No negotiations found for user with ID {userId}.");
            }

            return Ok(userNegotiations);
        }


        [HttpPost("accept-negotiation/{negotiationId}")]
        public async Task<IActionResult> AcceptNegotiation(int negotiationId)
        {
            var negotiation = await _context.Negotiations.FindAsync(negotiationId);

            if (negotiation == null)
            {
                return NotFound("Negotiation not found.");
            }

            // Update the negotiation status to "Accepted"
            negotiation.Status = "Accepted";
            _context.Negotiations.Update(negotiation);
            await _context.SaveChangesAsync();

            // Optionally, you might want to update the related product's price or order item's price here
            return Ok(new { Message = $"Negotiation {negotiationId} accepted." });
        }

        [HttpGet("all-negotiations")]
        public async Task<IActionResult> GetAllNegotiations()
        {
            var negotiations = await _context.Negotiations.ToListAsync();

            if (negotiations == null || !negotiations.Any())
            {
                return NotFound("No negotiations found.");
            }

            return Ok(negotiations);
        }

        [HttpGet("negotiations/accepted")]
        public async Task<IActionResult> GetAllAcceptedNegotiations()
        {
            var acceptedNegotiations = await _context.Negotiations
                .Where(n => n.Status == "Accepted")
                .ToListAsync();

            if (!acceptedNegotiations.Any())
            {
                return NotFound("No accepted negotiations found.");
            }

            return Ok(acceptedNegotiations);
        }

        [HttpGet("negotiations/pending")]
        public async Task<IActionResult> GetAllPendingNegotiations()
        {
            var pendingNegotiations = await _context.Negotiations
                .Where(n => n.Status == "Pending")
                .ToListAsync();

            if (!pendingNegotiations.Any())
            {
                return NotFound("No pending negotiations found.");
            }

            return Ok(pendingNegotiations);
        }

    }
}
