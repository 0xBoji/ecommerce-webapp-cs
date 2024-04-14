using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class productReviewsController : ControllerBase
{
    private readonly ArtsContext _context;

    public productReviewsController(ArtsContext context)
    {
        _context = context;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetAllReviewsByProductId(string productId)
    {
        var reviews = await _context.ProductReviews
            .Where(review => review.ProductId == productId)
            .Select(review => new
            {
                review.ReviewId,
                review.UserId,
                review.Rating,
                review.ReviewText,
                review.ReviewDate,
                review.IsApproved,
                UserFirstName = review.User.Firstname,
                UserLastName = review.User.Lastname
            })
            .ToListAsync();

        if (reviews == null || reviews.Count == 0)
        {
            return NotFound($"No reviews found for product with ID {productId}.");
        }

        return Ok(reviews);
    }


    // POST: api/ProductReviews
    [HttpPost]
    public async Task<IActionResult> AddProductReview([FromForm] ReviewCreateDto reviewDto)
    {
        var userIdString = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        // Ensure the rating is between 1 and 5
        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5 stars.");
        }

        if (await CanUserReviewProduct(userId, reviewDto.ProductId))
        {
            var review = new ProductReview
            {
                UserId = userId,
                ProductId = reviewDto.ProductId,
                Rating = reviewDto.Rating,
                ReviewText = reviewDto.ReviewText,
                ReviewDate = DateTime.UtcNow,
                IsApproved = true
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok("Review added successfully.");
        }
        else
        {
            return BadRequest("You must purchase the product before reviewing it.");
        }
    }


    private async Task<bool> CanUserReviewProduct(int userId, string productId)
    {
        var userOrders = await _context.Orders
            .Where(order => order.UserId == userId)
            .SelectMany(order => order.OrderItems)
            .Select(item => new OrderItemDtos
            {
                OrderItemId = item.OrderItemId,
                OrderId = item.OrderId,
                ProductId = item.ProId,
                Quantity = item.Quantity,
                Price = item.Price
            })
            .ToListAsync();

        var userHasPurchasedProduct = userOrders.Any(item => item.ProductId == productId);

        return userHasPurchasedProduct;
    }

}