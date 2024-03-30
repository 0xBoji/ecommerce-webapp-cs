/*using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductReviewsController : ControllerBase
{
	private readonly ArtsContext _context;

	public ProductReviewsController(ArtsContext context)
	{
		_context = context;
	}

	// POST: api/ProductReviews
	// This endpoint receives the userId, productId, rating, and reviewText for a new review
	[HttpPost]
	public async Task<IActionResult> AddProductReview(int userId, string productId, int rating, string reviewText)
	{
		if (await CanUserReviewProduct(userId, productId))
		{
			// User has purchased the product and can leave a review
			var review = new ProductReview
			{
				UserId = userId,
				ProductId = productId,
				Rating = rating,
				ReviewText = reviewText,
				ReviewDate = DateTime.UtcNow,
				IsApproved = false // Assuming reviews need approval
			};

			_context.ProductReviews.Add(review);
			await _context.SaveChangesAsync();

			return Ok("Review added successfully.");
		}
		else
		{
			// User hasn't purchased the product and cannot leave a review
			return BadRequest("You must purchase the product before reviewing it.");
		}
	}

	// This helper method checks if a user can review a product based on their purchase history
	private async Task<bool> CanUserReviewProduct(int userId, string productId)
	{
		// Check if the user has any order that includes the specified product
		var userHasPurchasedProduct = await _context.Orders
			.Include(o => o.OrderItems)
			.AnyAsync(order => order.UserId == userId &&
							   order.OrderItems.Any(item => item.ProductId == productId));

		return userHasPurchasedProduct;
	}
}
*/