using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class OrderItem
{
    public string OrderItemId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string ProId { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Pro { get; set; } = null!;
}
