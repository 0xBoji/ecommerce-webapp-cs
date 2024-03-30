using ecommerce_webapp_cs.Models.Entities;
using ecommerce_webapp_cs.Models.ProductModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_webapp_cs.Controllers;
[Route("api/[controller]")]
[ApiController]
public class refundRequestsController : ControllerBase
{
	private readonly ArtsContext _context;

	public refundRequestsController(ArtsContext context)
	{
		_context = context;
	}

	[HttpGet("pending")]
	public async Task<IActionResult> GetPendingRefundRequests()
	{
		var pendingRequests = await _context.ReturnRequests
											.Where(r => r.RequestStatus == "Pending")
											.Include(r => r.Order)
											.ThenInclude(o => o.User)
											.ToListAsync();

		return Ok(pendingRequests);
	}

	[Authorize(Roles = "Admin,Employee")]
	[HttpPost("process-refund")]
	public async Task<IActionResult> ProcessRefund([FromBody] ProcessRefundDto processRefundDto)
	{
		var order = await _context.Orders.FindAsync(processRefundDto.OrderId);
		if (order == null)
		{
			return NotFound("Order not found.");
		}
		var refundProcessed = ProcessRefundThroughGateway(processRefundDto);
		if (!refundProcessed)
		{
			return BadRequest("Refund processing failed.");
		}

		var refund = new Refund
		{
			OrderId = processRefundDto.OrderId,
			Amount = processRefundDto.Amount,
			RefundMethod = processRefundDto.RefundMethod,
			RefundStatus = "Completed",
			RefundDate = DateTime.UtcNow
		};

		_context.Refunds.Add(refund);
		await _context.SaveChangesAsync();

		return Ok(new { Message = "Refund processed successfully." });
	}

	private bool ProcessRefundThroughGateway(ProcessRefundDto processRefundDto)
	{
		// simulate refund processing
		//financial services API
		return true; // assume refund processing is successful for demonstration purposes
	}

}
