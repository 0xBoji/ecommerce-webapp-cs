namespace ecommerce_webapp_cs.Models.ProductModels;

public class ProductEditModel
{
	public string ProName { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
}
