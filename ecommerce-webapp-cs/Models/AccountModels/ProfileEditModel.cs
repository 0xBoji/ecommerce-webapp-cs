using System.ComponentModel.DataAnnotations;

namespace ecommerce_webapp_cs.Models.AccountModels;

public class ProfileEditModel
{
	public string Username { get; set; }
	public string PhoneNum { get; set; }

	public string FirstName { get; set; }
    public string LastName { get; set; }

    public string UserImg { get; set; }
	public string CompanyName { get; set; }
    public string AddressLine1 { get; set; }
    public string Country { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
}
