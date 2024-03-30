namespace ecommerce_webapp_cs.Models.DiscountModels;

public class VoucherCreateDto
{
	public string VoucherName { get; set; }
	public decimal Amount { get; set; }
	public string Code { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime Expired { get; set; }
	public string? Description { get; set; }
}
