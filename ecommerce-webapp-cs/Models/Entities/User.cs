using System;
using System.Collections.Generic;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNum { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public string? GoogleId { get; set; }

    public string? UserImg { get; set; }

    public string? CompanyName { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? Country { get; set; }

    public string? Province { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public bool EmailVerified { get; set; }

    public string? EmailVerificationToken { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public virtual ICollection<ChatMessage> ChatMessageFromUsers { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatMessage> ChatMessageToUsers { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Negotiation> Negotiations { get; set; } = new List<Negotiation>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ChatSession> Sessions { get; set; } = new List<ChatSession>();
}
