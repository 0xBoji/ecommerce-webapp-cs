using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class AuditLog
{
    public int LogId { get; set; }

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? ActionDescription { get; set; }

    public DateTime ActionDate { get; set; }

    public int? ActionBy { get; set; }

    public virtual User? ActionByNavigation { get; set; }
}
