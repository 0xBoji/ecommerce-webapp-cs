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
using System.Net.Mail;

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
                EmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString()
            };

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(Profile), new { userId = user.UserId }, user);
		}

		return BadRequest(ModelState);
	}

    private async Task SendVerificationEmail(string email, string link)
    {
        using (var client = new SmtpClient())
        {
            // Configure your SMTP client settings (host, port, credentials...)

            var mailMessage = new MailMessage
            {
                From = new MailAddress("pichstudent2004@gmail.com"),
                Subject = "Verify your email",
                Body = $"<a href=\"{link}\">Click here to verify your email</a>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }

    [HttpGet("verifyemail")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null)
        {
            return NotFound("Verification failed. Invalid token.");
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null; // Clear the token after verification

        await _context.SaveChangesAsync();

        return Ok("Email verified successfully.");
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

    [HttpPost("login-admin")]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || user.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    HttpContext.Session.SetString("UserID", user.UserId.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    return Ok(new { message = "Login successful" });
                }
                else
                {
                    return Unauthorized(new { message = "Access denied. User does not have the required role." });
                }
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

        var model = new ProfileModel
        {
            Username = user.Username,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
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


    private ProfileModel MapToProfileModel(User user)
    {
        return new ProfileModel
        {
            Username = user.Username,
            Firstname = user.Firstname,
            Lastname = user.Lastname,
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

    [HttpPost("profile/edit")]
    public async Task<IActionResult> UpdateProfile([FromForm] ProfileEditModel model)
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

        // Check for username and phone number conflicts
        bool usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != userId);
        bool phoneNumExists = await _context.Users.AnyAsync(u => u.PhoneNum == model.PhoneNum && u.UserId != userId);
        if (usernameExists)
        {
            return BadRequest(new { message = "Username already in use by another account." });
        }
        if (phoneNumExists)
        {
            return BadRequest(new { message = "Phone number already in use by another account." });
        }

        try
        {
            if (ModelState.IsValid)
            {
                user.Username = model.Username;
                user.Firstname = model.Firstname;
                user.Lastname = model.Lastname;
                user.PhoneNum = model.PhoneNum;

                if (model.UserImg != null)
                {
                    if (!model.UserImg.ContentType.StartsWith("image/"))
                    {
                        return BadRequest(new { message = "Only image files are allowed." });
                    }


                    var imagePath = await SaveUserImage(model.UserImg);
                    user.UserImg = imagePath; 
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile updated successfully" });
            }
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the profile. Please try again." });
        }
    }

    private async Task<string> SaveUserImage(IFormFile imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            // check if the file is an image
            if (!imageFile.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("Only image files are allowed.");
            }

            // check if the image size is less than 5MB
            if (imageFile.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Image size cannot exceed 5MB.");
            }

            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "UserImages");

            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            var extension = Path.GetExtension(imageFile.FileName);
            var newFileName = $"{Guid.NewGuid()}{extension}"; // generate a new file name to prevent overwriting
            var filePath = Path.Combine(imagesPath, newFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return newFileName; // returning the new file name or a relative path as needed
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the image file.", ex);
            }
        }

        return null; // return null if no image was provided or if it didn't pass the checks
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

			var profileModel = new ProfileModel
            {
                Username = user.Username,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
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
        [HttpGet("session/userId")]
        public IActionResult GetUserIdFromSession()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            return Ok(new { UserId = userId });
        }
}
