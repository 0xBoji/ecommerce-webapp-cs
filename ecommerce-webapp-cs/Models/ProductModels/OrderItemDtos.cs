namespace ecommerce_webapp_cs.Models.ProductModels;

public class OrderItemDtos
{
	public string OrderItemId { get; set; } = null!;
	public string OrderId { get; set; } = null!;
	public string ProductId { get; set; } = null!;
	public int Quantity { get; set; }
	public decimal Price { get; set; }
}
