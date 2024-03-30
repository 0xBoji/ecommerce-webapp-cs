namespace ecommerce_webapp_cs.Models.ProductModels;

public class CartItemDto
{
	public int UserId { get; set; }
	public string ProId { get; set; }
	public int Quantity { get; set; }
}
