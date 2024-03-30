using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce_webapp_cs.Models.ProductModels;
using System.Data.SqlClient;

namespace ecommerce_webapp_cs.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class productsController : ControllerBase
{
	private readonly ArtsContext _context;

	public productsController(ArtsContext context)
	{
		_context = context;
	}

	// GET: api/Products
	[HttpGet]
	public async Task<IActionResult> GetProducts()
	{
		var products = await _context.Products.Include(p => p.Category).ToListAsync();
		return Ok(products);
	}

	// GET: api/Products/5
	[HttpGet("{id}")]
	public async Task<IActionResult> GetProduct(string id)
	{
		try { 
		var product = await _context.Products.Include(p => p.Category)
			.FirstOrDefaultAsync(p => p.ProId == id);

		if (product == null)
		{
			return NotFound();
		}

		return Ok(product);
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// POST: api/Products
	[HttpPost]
	public async Task<IActionResult> PostProduct([FromBody] ProductModel productDto)
	{
		try
		{
			// Map ProductCreateDto to Product entity
			var product = new Product
			{
				ProName = productDto.ProName,
				Description = productDto.Description,
				Price = productDto.Price,
				StockQuantity = productDto.StockQuantity,
				CategoryId = productDto.CategoryId
			};

			// Check if the CategoryId is valid
			var category = await _context.ProductCategories.FindAsync(productDto.CategoryId);
			if (category == null)
			{
				return BadRequest("Invalid CategoryId");
			}

			// Generate ProId
			product.ProId = $"{product.CategoryId}{GenerateRandomString(3)}";

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetProduct", new { id = product.ProId }, product);
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// PUT: api/Products/5
	[HttpPut("{id}")]
	public async Task<IActionResult> PutProduct(string id, [FromBody] ProductEditModel productDto)
	{
		try
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			product.ProName = productDto.ProName;
			product.Description = productDto.Description;
			product.Price = productDto.Price;
			product.StockQuantity = productDto.StockQuantity;
			_context.Products.Update(product);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// DELETE: api/Products/5
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteProduct(string id)
	{
		try
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

			return Ok(product);
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	private bool ProductExists(string id)
	{
		return _context.Products.Any(e => e.ProId == id);
	}

	private string GenerateRandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		var random = new Random();
		return new string(Enumerable.Repeat(chars, length)
		  .Select(s => s[random.Next(s.Length)]).ToArray());
	}
	// GET: api/v1/Products/Categories
	[HttpGet("categories")]
	public async Task<IActionResult> GetCategories()
	{
		var categories = await _context.ProductCategories.ToListAsync();
		return Ok(categories);
	}

	// GET: api/v1/Products/Categories/5
	[HttpGet("categories/{id}")]
	public async Task<IActionResult> GetCategory(int id)
	{
		try
		{
			var category = await _context.ProductCategories.FindAsync(id);

			if (category == null)
			{
				return NotFound();
			}

			return Ok(category);

		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// POST: api/v1/Products/Categories
	[HttpPost("categories")]
	public async Task<IActionResult> PostCategory([FromBody] CategoryModel categoryDto)
	{
		try
		{
			var category = new ProductCategory
			{
				CategoryName = categoryDto.CategoryName
			};

			_context.ProductCategories.Add(category);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// PUT: api/v1/Products/Categories/5
	[HttpPut("categories/{id}")]
	public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryModel categoryDto)
	{
		try
		{
			var category = await _context.ProductCategories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			category.CategoryName = categoryDto.CategoryName;

			_context.ProductCategories.Update(category);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.ProductCategories.Any(e => e.CategoryId == id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}
		catch (DbUpdateException ex)
		{
			return HandleDbUpdateException(ex);
		}
	}

	// DELETE: api/v1/Products/Categories/5
	[HttpDelete("Categories/{id}")]
	public async Task<IActionResult> DeleteCategory(int id)
	{
		var category = await _context.ProductCategories.FindAsync(id);
		if (category == null)
		{
			return NotFound();
		}

		try
		{
			_context.ProductCategories.Remove(category);
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateException ex)
		{
			// Check if the exception is due to a foreign key constraint
			if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2627))
			{
				return BadRequest("Cannot delete this category because it is associated with one or more discounts.");
			}
			throw; // If it's not the expected exception type or code, rethrow it
		}

		return Ok("Category deleted successfully.");
	}


	private IActionResult HandleDbUpdateException(DbUpdateException ex)
	{
		if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 547 || sqlEx.Number == 2627))
		{
			return BadRequest("Operation failed due to a database constraint. Details: " + sqlEx.Message);
		}
		else
		{
			return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Details: " + ex.Message);
		}
	}


}