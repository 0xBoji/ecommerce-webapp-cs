using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class Product
{
    public string ProId { get; set; } = null!;

    public string ProName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ProImg1 { get; set; }

    public string? ProImg2 { get; set; }

    public string? ProImg3 { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public DateTime? CreationDate { get; set; }

    public virtual ICollection<Negotiation> Negotiations { get; set; } = new List<Negotiation>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}