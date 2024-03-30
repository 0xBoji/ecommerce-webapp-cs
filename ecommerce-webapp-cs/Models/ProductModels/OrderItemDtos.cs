namespace ecommerce_webapp_cs.Models.ProductModels;

public class OrderItemDtos
{
	public string OrderItemId { get; set; } = null!;
	public string OrderId { get; set; } = null!;
	public string ProductId { get; set; } = null!; // This matches the expected name in your method
	public int Quantity { get; set; }
	public decimal Price { get; set; }
}
