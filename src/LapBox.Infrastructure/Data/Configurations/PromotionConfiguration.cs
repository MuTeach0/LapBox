using LapBox.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");

        builder.HasKey(p => p.Id).IsClustered(false);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DiscountPercentage)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // AuditableEntity
        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(100);
        builder.Property(p => p.LastModifiedUtc).IsRequired();
        builder.Property(p => p.LastModifiedBy).HasMaxLength(100);
    }
}
