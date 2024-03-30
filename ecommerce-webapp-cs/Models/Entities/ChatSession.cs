using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class ChatSession
{
    public int SessionId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
