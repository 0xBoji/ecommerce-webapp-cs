using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class ReturnRequest
{
    public int RequestId { get; set; }

    public string OrderId { get; set; } = null!;

    public string? Reason { get; set; }

    public string RequestStatus { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
