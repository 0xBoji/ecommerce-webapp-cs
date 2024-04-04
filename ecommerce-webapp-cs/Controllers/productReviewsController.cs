using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class productreviewsController : ControllerBase
{
	private readonly ArtsContext _context;

	public productreviewsController(ArtsContext context)
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
	public async Task<IActionResult> AddProductReview(int userId, string productId, int rating, string reviewText)
	{
		if (await CanUserReviewProduct(userId, productId))
		{
			var review = new ProductReview
			{
				UserId = userId,
				ProductId = productId,
				Rating = rating,
				ReviewText = reviewText,
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
