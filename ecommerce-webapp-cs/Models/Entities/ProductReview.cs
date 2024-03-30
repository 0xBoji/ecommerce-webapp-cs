using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class ProductReview
{
    public int ReviewId { get; set; }

    public string ProductId { get; set; } = null!;

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string ReviewText { get; set; } = null!;

    public DateTime ReviewDate { get; set; }

    public bool IsApproved { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
