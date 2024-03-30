namespace ecommerce_webapp_cs.Models.ProductModels;
public class OrderCreationDto
{
	public int UserId { get; set; } 
	public string? Note { get; set; }

	public string VoucherCode { get; set; }
	public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>(); 
}

public class OrderItemDto
{
	public string ProId { get; set; }
	public int Quantity { get; set; } 
}