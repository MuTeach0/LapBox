using LapBox.Domain.Payments;
using LapBox.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id).IsClustered(false);
        builder.HasIndex(p => p.OrderId).IsUnique();

        builder.Property(p => p.OrderId).IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("EGP");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ExternalTransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        // Enforce FK to Orders (Payment belongs to an Order) — model uses OrderId
        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditableEntity
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.LastModifiedUtc).IsRequired();
        builder.Property(p => p.LastModifiedBy).HasMaxLength(100);
    }
}
