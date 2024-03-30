using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Models.ProductModels;

public class ProductModel
{
	public string ProName { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	public int CategoryId { get; set; }
}
