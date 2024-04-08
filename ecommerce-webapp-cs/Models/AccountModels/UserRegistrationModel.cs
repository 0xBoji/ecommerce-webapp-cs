using System.ComponentModel.DataAnnotations;

namespace ecommerce_webapp_cs.Models.AccountModels;

public class UserRegistrationModel
{
	[Required, EmailAddress, Display(Name = "Email")]
	public string Email { get; set; }

	[Required, Display(Name = "Username")]
	public string Username { get; set; }

	[Display(Name = "Phone Number")]
	public string PhoneNum { get; set; }


    [Required, DataType(DataType.Password), Display(Name = "Password")]
	public string Password { get; set; }

	[DataType(DataType.Password), Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
	[Display(Name = "Confirm Password")]
	public string ConfirmPassword { get; set; }
}
