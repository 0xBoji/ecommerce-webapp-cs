namespace ecommerce_webapp_cs.Models.ProductModels;

public class ProcessRefundDto
{
	public string OrderId { get; set; }
	public decimal Amount { get; set; }
	public string RefundMethod { get; set; }
}
