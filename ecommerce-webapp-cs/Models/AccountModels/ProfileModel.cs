using System.ComponentModel.DataAnnotations;

namespace ecommerce_webapp_cs.Models.AccountModels;

public class ProfileModel
{
	public string Username { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string PhoneNum { get; set; }
	public string UserImg { get; set; }
}
