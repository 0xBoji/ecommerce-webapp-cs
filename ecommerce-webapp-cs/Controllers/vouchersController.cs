using ecommerce_webapp_cs.Models.DiscountModels;
using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_webapp_cs.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class vouchersController : ControllerBase
{
	private readonly ArtsContext _context;

	public vouchersController(ArtsContext context)
	{
		_context = context;
	}

	// GET: api/v1/Vouchers
	[HttpGet]
	public async Task<IActionResult> GetAllVouchers()
	{
		var vouchers = await _context.Vouchers.ToListAsync();
		return Ok(vouchers);
	}

	// GET: api/v1/Vouchers/5
	[HttpGet("{id}")]
	public async Task<IActionResult> GetVoucher(int id)
	{
		var voucher = await _context.Vouchers.FindAsync(id);

		if (voucher == null)
		{
			return NotFound();
		}

		return Ok(voucher);
	}

	// POST: api/v1/Vouchers
	[HttpPost]
	public async Task<IActionResult> CreateVoucher([FromBody] VoucherCreateDto voucherDto)
	{
		var voucher = new Voucher
		{
			VoucherName = voucherDto.VoucherName,
			Amount = voucherDto.Amount,
			Code = voucherDto.Code,
			StartDate = voucherDto.StartDate,
			Expired = voucherDto.Expired,
			Description = voucherDto.Description
		};

		_context.Vouchers.Add(voucher);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetVoucher), new { id = voucher.VoucherId }, voucher);
	}
	// PUT: api/v1/Vouchers/5
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateVoucher(int id, [FromBody] Voucher voucher)
	{
		if (id != voucher.VoucherId)
		{
			return BadRequest();
		}

		_context.Entry(voucher).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!_context.Vouchers.Any(e => e.VoucherId == id))
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

	// DELETE: api/v1/Vouchers/5
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteVoucher(int id)
	{
		var voucher = await _context.Vouchers.FindAsync(id);
		if (voucher == null)
		{
			return NotFound();
		}

		_context.Vouchers.Remove(voucher);
		await _context.SaveChangesAsync();

		return NoContent();
	}
}
