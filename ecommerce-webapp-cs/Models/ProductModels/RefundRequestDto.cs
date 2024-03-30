namespace ecommerce_webapp_cs.Models.ProductModels;

public class RefundRequestDto
{
	public string OrderId { get; set; }
	public int UserId { get; set; }
	public string Reason { get; set; }
}
