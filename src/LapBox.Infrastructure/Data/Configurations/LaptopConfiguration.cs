using LapBox.Domain.Laptops;
using LapBox.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class LaptopConfiguration : IEntityTypeConfiguration<Laptop>
{
    public void Configure(EntityTypeBuilder<Laptop> builder)
    {
        builder.ToTable("Laptops");

        builder.HasKey(l => l.Id).IsClustered(false);
        builder.HasIndex(l => l.BrandId);
        builder.HasIndex(l => l.CategoryId);
        builder.HasIndex(l => l.IsActive);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(l => l.Sku).IsUnique();

        builder.Property(l => l.Description)
            .HasMaxLength(2000);

        builder.Property(l => l.BasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.InventoryQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation properties - using string navigation to avoid null reference
        builder.HasOne<Brand>()
            .WithMany()
            .HasForeignKey(l => l.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owned entity - Specification
        builder.OwnsOne(l => l.Specification, spec =>
        {
            spec.Property(s => s.Processor).HasMaxLength(100).IsRequired();
            spec.Property(s => s.RAM).HasMaxLength(50).IsRequired();
            spec.Property(s => s.Storage).HasMaxLength(50).IsRequired();
            spec.Property(s => s.ScreenSize).HasMaxLength(50).IsRequired();
            spec.Property(s => s.GraphicsCard).HasMaxLength(100).IsRequired();
        });

        // Owned collection - Images
        builder.OwnsMany(l => l.Images, img =>
        {
            img.ToTable("LaptopImages");
            img.WithOwner().HasForeignKey("LaptopId");
            img.HasKey("LaptopId", "Url");
            img.Property(i => i.Url).HasMaxLength(500).IsRequired();
            img.Property(i => i.IsMain).IsRequired().HasDefaultValue(false);
            img.Property(i => i.DisplayOrder).IsRequired().HasDefaultValue(0);
        });

        builder.Navigation(l => l.Images)
            .Metadata.SetField("_images");
        builder.Navigation(l => l.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // AuditableEntity
        builder.Property(l => l.CreatedAtUtc).IsRequired();
        builder.Property(l => l.CreatedBy).HasMaxLength(100);
        builder.Property(l => l.LastModifiedUtc).IsRequired();
        builder.Property(l => l.LastModifiedBy).HasMaxLength(100);
    }
}
