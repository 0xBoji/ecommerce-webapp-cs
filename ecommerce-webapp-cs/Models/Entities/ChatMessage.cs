using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class ChatMessage
{
    public int MessageId { get; set; }

    public int SessionId { get; set; }

    public int FromUserId { get; set; }

    public int ToUserId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public bool IsRead { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual ChatSession Session { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;
}
