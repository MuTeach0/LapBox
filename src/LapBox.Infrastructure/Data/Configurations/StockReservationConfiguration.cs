using LapBox.Domain.Laptops;
using LapBox.Domain.Orders;
using LapBox.Domain.StockReservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(r => r.Id).IsClustered(false);
        builder.HasIndex(r => r.LaptopId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ExpiresAtUtc);

        builder.Property(r => r.LaptopId).IsRequired();
        builder.Property(r => r.UserId).IsRequired(); // This is the Identity/UserId
        builder.Property(r => r.OrderId); // Nullable - set when Order is created
        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.ExpiresAtUtc).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Enforce referential integrity for StockReservations
        builder.HasOne<Laptop>()
            .WithMany()
            .HasForeignKey(r => r.LaptopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditableEntity
        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedUtc).IsRequired();
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);
    }
}
