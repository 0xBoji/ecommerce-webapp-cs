using ecommerce_webapp_cs.Models.Entities;

namespace ecommerce_webapp_cs.Models.ProductModels;

public class ProductModel
{
    public string ProName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public IFormFile ProImg1 { get; set; }
    public IFormFile ProImg2 { get; set; }
    public IFormFile ProImg3 { get; set; }
    public int StockQuantity { get; set; }
}