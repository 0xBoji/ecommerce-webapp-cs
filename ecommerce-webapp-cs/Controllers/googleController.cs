using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase
{
    private readonly ArtsContext _context;
    private readonly IConfiguration _configuration;

    public GoogleController(ArtsContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration; // Inject configuration to access JWT settings
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var redirectUrl = Url.Action(nameof(Callback));
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            return BadRequest("Google authentication failed.");
        }

        var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
        var googleId = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(name))
        {
            return BadRequest("Required information is missing from the Google authentication response.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Username = name,
                Email = email,
                GoogleId = googleId,
                CreateDate = DateTime.UtcNow,
                Role = "Customer",
                PhoneNum = "Null"
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

        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Redirect to the client application with the token
        return Redirect($"https://localhost:7060/?token={token}");
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                // Add more claims as needed
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
