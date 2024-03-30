using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using	ecommerce_webapp_cs.Models.Entities; 

namespace ecommerce_webapp_cs.Controllers;

[ApiController]
[Route("[controller]")]
public class googleController : ControllerBase
{
	private readonly ArtsContext _context; 

	public googleController(ArtsContext context)
	{
		_context = context;
	}

	[HttpGet("login")]
	public IActionResult Login()
	{
		var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(Callback)) };
		return Challenge(properties, GoogleDefaults.AuthenticationScheme);
	}

	[HttpGet("callback")]
	public async Task<IActionResult> Callback()
	{
		var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

		if (!authenticateResult.Succeeded) return BadRequest("Google authentication failed.");

		var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
		var googleId = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
		var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

		var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

		if (user == null)
		{
			user = new User
			{
				Username = name,
				Email = email,
				GoogleId = googleId,
				CreateDate = DateTime.UtcNow,
			};
			_context.Users.Add(user);
		}
		else
		{
			if (string.IsNullOrEmpty(user.GoogleId))
			{
				user.GoogleId = googleId;
				user.LastLoginDate = DateTime.UtcNow;
			}
		}

		await _context.SaveChangesAsync();

		return Redirect("/");
	}
}
