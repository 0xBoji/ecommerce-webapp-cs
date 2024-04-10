using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_webapp_cs.Models.Entities;

public partial class ArtsContext : DbContext
{
    public ArtsContext()
    {
    }

    public ArtsContext(DbContextOptions<ArtsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<Negotiation> Negotiations { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<ReturnRequest> ReturnRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DEVBLOCK; Initial Catalog=Arts;Persist Security Info=True;User ID=sa;Password=05012004;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__AuditLog__5E5499A8BADE0EBC");

            entity.ToTable("AuditLog");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ActionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionDescription).IsUnicode(false);
            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ActionByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ActionBy)
                .HasConstraintName("FK__AuditLog__Action__35DCF99B");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__BlogComm__C3B4DFAA47F05230");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.CommentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Post).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__PostI__3D7E1B63");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__UserI__3E723F9C");
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__AA126038E3C9FB66");

            entity.Property(e => e.PostId).HasColumnName("PostID");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.PostImg).HasMaxLength(255);
            entity.Property(e => e.PostedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogPosts__UserI__39AD8A7F");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ChatMess__C87C037C78D206A6");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.FromUserId).HasColumnName("FromUserID");
            entity.Property(e => e.MessageText).IsUnicode(false);
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ToUserId).HasColumnName("ToUserID");

            entity.HasOne(d => d.FromUser).WithMany(p => p.ChatMessageFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__FromU__1D114BD1");

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__Sessi__1C1D2798");

            entity.HasOne(d => d.ToUser).WithMany(p => p.ChatMessageToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChatMessa__ToUse__1E05700A");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__ChatSess__C9F49270B1298E92");

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasMany(d => d.Users).WithMany(p => p.Sessions)
                .UsingEntity<Dictionary<string, object>>(
                    "SessionParticipant",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SessionPa__UserI__21D600EE"),
                    l => l.HasOne<ChatSession>().WithMany()
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__SessionPa__Sessi__20E1DCB5"),
                    j =>
                    {
                        j.HasKey("SessionId", "UserId").HasName("PK__SessionP__188C1EBAA04E6972");
                        j.ToTable("SessionParticipants");
                        j.IndexerProperty<int>("SessionId").HasColumnName("SessionID");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                    });
        });

        modelBuilder.Entity<Negotiation>(entity =>
        {
            entity.HasKey(e => e.NegotiationId).HasName("PK__Negotiat__4C30331F6C5441F5");

            entity.Property(e => e.NegotiationId).HasColumnName("NegotiationID");
            entity.Property(e => e.LastUpdated).HasColumnType("datetime");
            entity.Property(e => e.NegotiatedPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NegotiationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProId)
                .HasMaxLength(7)
                .HasColumnName("ProID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Pro).WithMany(p => p.Negotiations)
                .HasForeignKey(d => d.ProId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Negotiati__ProID__4242D080");

            entity.HasOne(d => d.User).WithMany(p => p.Negotiations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Negotiati__UserI__4336F4B9");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF09C691A0");

            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .HasColumnName("OrderID");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReturnStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("None");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserID__07220AB2");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A195B413BD");

            entity.Property(e => e.OrderItemId)
                .HasMaxLength(8)
                .HasColumnName("OrderItemID");
            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .HasColumnName("OrderID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProId)
                .HasMaxLength(7)
                .HasColumnName("ProID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__0BE6BFCF");

            entity.HasOne(d => d.Pro).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__ProID__0CDAE408");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58037977AC");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .HasColumnName("OrderID");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefundStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("None");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__OrderI__119F9925");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProId).HasName("PK__Products__620295F006A6C81A");

            entity.Property(e => e.ProId)
                .HasMaxLength(7)
                .HasColumnName("ProID");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProImg1)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProImg2)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProImg3)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__ProductR__74BC79AECA25D60A");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(7)
                .HasColumnName("ProductID");
            entity.Property(e => e.ReviewDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReviewText).HasColumnType("text");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductRe__Produ__269AB60B");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductRe__UserI__278EDA44");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.RefundId).HasName("PK__Refunds__725AB90027EA8A42");

            entity.Property(e => e.RefundId).HasColumnName("RefundID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .HasColumnName("OrderID");
            entity.Property(e => e.RefundDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RefundMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefundStatus)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Order).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Refunds__OrderID__2B5F6B28");
        });

        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__ReturnRe__33A8519ABD946F2D");

            entity.Property(e => e.RequestId).HasColumnName("RequestID");
            entity.Property(e => e.OrderId)
                .HasMaxLength(16)
                .HasColumnName("OrderID");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RequestStatus)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Order).WithMany(p => p.ReturnRequests)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReturnReq__Order__3118447E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC31450F6C");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4F94CCDDF").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534C9D634B7").IsUnique();

            entity.HasIndex(e => e.PhoneNum, "UQ__Users__DF8F1A02E4847944").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EmailVerificationToken)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Firstname)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.Lastname)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNum)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.PostalCode)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Province)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserImg)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("User_img");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
