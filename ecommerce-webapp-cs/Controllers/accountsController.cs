using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ecommerce_webapp_cs.Models;
using ecommerce_webapp_cs.Models.Entities; 
using BCrypt.Net;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ecommerce_webapp_cs.Models.AccountModels;
using System.Security.Claims;
using System.Diagnostics.Metrics;

namespace ecommerce_webapp_cs.Controllers;
[Route("api/v1/[controller]")]
[ApiController]
public class accountsController : ControllerBase
{
	private readonly ArtsContext _context;

	public accountsController(ArtsContext context)
	{
		_context = context;
	}

	// SignUp endpoint
	[HttpPost("signup")]
	public async Task<IActionResult> SignUp([FromBody] UserRegistrationModel model)
	{
		if (ModelState.IsValid)
		{
			var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
			var usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);
			var phoneNumExists = await _context.Users.AnyAsync(u => u.PhoneNum == model.PhoneNum);

			if (emailExists || usernameExists || phoneNumExists)
			{
				return BadRequest(new { message = "User with given details already exists" });
			}

			var user = new User
			{
				Username = model.Username,
				Email = model.Email,
				PhoneNum = model.PhoneNum,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
				Role = "Customer",
				CreateDate = DateTime.UtcNow,
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(Profile), new { userId = user.UserId }, user);
		}

		return BadRequest(ModelState);
	}

	// Login endpoint
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		if (ModelState.IsValid)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

			if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
			{
		
				HttpContext.Session.SetString("UserID", user.UserId.ToString());
				HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Useremail", user.Email);


                return Ok(new { message = "Login successful" });
			}

			return Unauthorized(new { message = "Invalid login attempt" });
		}

		return BadRequest(ModelState);
	}

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        User user = null;

        var userIdString = HttpContext.Session.GetString("UserID");
        if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
        {
            user = await _context.Users.FindAsync(userId);
        }
        else if (User.Identity.IsAuthenticated)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }
        }

        if (user == null)
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        var model = new ProfileEditModel
        {
            Username = user.Username,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            PhoneNum = user.PhoneNum,
            UserImg = user.UserImg,
			CompanyName = user.CompanyName,
			AddressLine1 = user.AddressLine1,
            Country = user.Country,
            Province = user.Province,
            City = user.City,
            PostalCode = user.PostalCode

        };

        return Ok(model);
    }


    private ProfileEditModel MapToProfileModel(User user)
    {
        return new ProfileEditModel
        {
            Username = user.Username,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            PhoneNum = user.PhoneNum,
            UserImg = user.UserImg,
            CompanyName = user.CompanyName,
            AddressLine1 = user.AddressLine1,
            Country = user.Country,
            Province = user.Province,
            City = user.City,
            PostalCode = user.PostalCode
        };
    }


    [HttpPost("logout")]
	public IActionResult Logout()
	{
		HttpContext.Session.Clear(); // clears all data stored in session
		return Ok(new { message = "You have been logged out successfully" });
	}




	[HttpPut("profile/edit")]
		public async Task<IActionResult> EditProfile([FromBody] ProfileEditModel model)
		{
			var userIdString = HttpContext.Session.GetString("UserID");
			if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
			{
				return Unauthorized(new { message = "User is not authenticated" });
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound(new { message = "User not found" });
			}

			var usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != userId);
			var phoneNumExists = await _context.Users.AnyAsync(u => u.PhoneNum == model.PhoneNum && u.UserId != userId);
			if (usernameExists || phoneNumExists)
			{
				return BadRequest(new { message = "Username or Phone Number already in use by another account." });
			}

			if (ModelState.IsValid)
			{
				user.Username = model.Username;
				user.Firstname = model.FirstName;
				user.Lastname = model.LastName;
				user.PhoneNum = model.PhoneNum;
				user.UserImg = model.UserImg;
				user.CompanyName = model.CompanyName;
				user.AddressLine1 = model.AddressLine1;
				user.Country = model.Country;
				user.Province = model.Province;
				user.City = model.City;
				user.PostalCode = model.PostalCode;


                await _context.SaveChangesAsync();

				return Ok(new { message = "Profile updated successfully" });
			}

			return BadRequest(ModelState);
		}


		[HttpGet("profile/edit")]
		public async Task<IActionResult> GetProfileForEdit()
		{
			var userIdString = HttpContext.Session.GetString("UserID");
			if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
			{
				return Unauthorized(new { message = "User is not authenticated" });
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return NotFound(new { message = "User not found" });
			}

			var profileModel = new ProfileEditModel
            {
				Username = user.Username,
				FirstName = user.Firstname,
				LastName = user.Lastname,
				PhoneNum = user.PhoneNum,
				UserImg = user.UserImg,
                CompanyName = user.CompanyName,
                AddressLine1 = user.AddressLine1,
                Country = user.Country,
                Province = user.Province,
                City = user.City,
                PostalCode = user.PostalCode
            };

			return Ok(profileModel);
		}

	}
