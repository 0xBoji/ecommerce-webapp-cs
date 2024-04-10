namespace ecommerce_webapp_cs.Models.ProductModels;

public class ReviewCreateDto
{
    public string ProductId { get; set; }

    public int Rating { get; set; } // Rating provided by the user for the product.

    public string ReviewText { get; set; }
}
