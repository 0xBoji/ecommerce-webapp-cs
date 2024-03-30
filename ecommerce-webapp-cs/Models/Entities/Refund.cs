using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class Refund
{
    public int RefundId { get; set; }

    public string OrderId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? RefundMethod { get; set; }

    public string RefundStatus { get; set; } = null!;

    public DateTime RefundDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
