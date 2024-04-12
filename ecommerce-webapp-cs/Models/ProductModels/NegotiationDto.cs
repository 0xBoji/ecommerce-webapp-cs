namespace ecommerce_webapp_cs.Models.ProductModels;

public class NegotiationDto
{
    public string ProId { get; set; } // Product ID for which the negotiation is being made
    public decimal NegotiatedPrice { get; set; } // The price being proposed in the negotiation
}
