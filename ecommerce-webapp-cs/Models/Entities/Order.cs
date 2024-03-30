using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public decimal TotalPrice { get; set; }

    public int? VoucherId { get; set; }

    public string? ReturnStatus { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
