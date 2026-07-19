using LapBox.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id).IsClustered(false);
        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // AuditableEntity
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.LastModifiedUtc).IsRequired();
        builder.Property(c => c.LastModifiedBy).HasMaxLength(100);
    }
}
