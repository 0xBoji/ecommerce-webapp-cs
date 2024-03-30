using ecommerce_webapp_cs.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce_webapp_cs.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class auditLogController : ControllerBase
{
	private readonly ArtsContext _context;

	public auditLogController(ArtsContext context)
	{
		_context = context;
	}

	// GET: api/v1/AuditLog
	[HttpGet]
	public async Task<IActionResult> GetAuditLogs()
	{
		var logs = await _context.AuditLogs
								 .Include(log => log.ActionByNavigation)
								 .OrderByDescending(log => log.ActionDate)
								 .ToListAsync();
		return Ok(logs);
	}

	// GET: api/v1/AuditLog/5
	[HttpGet("{id}")]
	public async Task<IActionResult> GetAuditLog(int id)
	{
		var auditLog = await _context.AuditLogs
									 .Include(log => log.ActionByNavigation)
									 .FirstOrDefaultAsync(log => log.LogId == id);

		if (auditLog == null)
		{
			return NotFound();
		}

		return Ok(auditLog);
	}

	[HttpPost]
	public async Task<IActionResult> CreateAuditLog([FromBody] AuditLog auditLog)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		_context.AuditLogs.Add(auditLog);
		await _context.SaveChangesAsync();

		return CreatedAtAction("GetAuditLog", new { id = auditLog.LogId }, auditLog);
	}

}
