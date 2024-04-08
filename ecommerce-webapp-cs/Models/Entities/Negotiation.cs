using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class Negotiation
{
    public int NegotiationId { get; set; }

    public string ProId { get; set; } = null!;

    public int UserId { get; set; }

    public decimal NegotiatedPrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime NegotiationDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Product Pro { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
