using LapBox.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id).IsClustered(false);
        builder.HasIndex(b => b.Name).IsUnique();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // AuditableEntity
        builder.Property(b => b.CreatedAtUtc).IsRequired();
        builder.Property(b => b.CreatedBy).HasMaxLength(100);
        builder.Property(b => b.LastModifiedUtc).IsRequired();
        builder.Property(b => b.LastModifiedBy).HasMaxLength(100);
    }
}
