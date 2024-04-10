using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Models.ProductModels;

public class ProductModel
{
	public string ProName { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
    public string ProImg1 { get; set; }
    public string ProImg2 { get; set; }
    public string ProImg3 { get; set; }
    public int StockQuantity { get; set; }
}
