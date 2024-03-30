using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_webapp_cs.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class discountsController : ControllerBase
{
	private readonly ArtsContext _context;

	public discountsController(ArtsContext context)
	{
		_context = context;
	}

	// GET: api/v1/Discounts
	[HttpGet]
	public async Task<IActionResult> GetAllDiscounts()
	{
		var discounts = await _context.Discounts.ToListAsync();
		return Ok(discounts);
	}

	// GET: api/v1/Discounts/5
	[HttpGet("{id}")]
	public async Task<IActionResult> GetDiscount(int id)
	{
		var discount = await _context.Discounts.FindAsync(id);

		if (discount == null)
		{
			return NotFound();
		}

		return Ok(discount);
	}

	// POST: api/v1/Discounts
	[HttpPost]
	public async Task<IActionResult> CreateDiscount([FromBody] Discount discount)
	{
		_context.Discounts.Add(discount);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetDiscount), new { id = discount.DiscountId }, discount);
	}

	// PUT: api/v1/Discounts/5
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateDiscount(int id, [FromBody] Discount discount)
	{
		if (id != discount.DiscountId)
		{
			return BadRequest();
		}

		_context.Entry(discount).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!_context.Discounts.Any(e => e.DiscountId == id))
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

	// DELETE: api/v1/Discounts/5
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteDiscount(int id)
	{
		var discount = await _context.Discounts.FindAsync(id);
		if (discount == null)
		{
			return NotFound();
		}

		_context.Discounts.Remove(discount);
		await _context.SaveChangesAsync();

		return NoContent();
	}

}
