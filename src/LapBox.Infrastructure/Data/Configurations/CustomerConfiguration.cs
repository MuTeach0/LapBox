using LapBox.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id).IsClustered(false);
        builder.Property(c => c.IdentityId).IsRequired();
        builder.HasIndex(c => c.IdentityId).IsUnique();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Email).IsRequired() .HasMaxLength(150);
        builder.HasIndex(c => c.Email).IsUnique();

        builder.Property(c => c.PhoneNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(c => c.TotalOrdersCount).IsRequired().HasDefaultValue(0);
        builder.Property(c => c.FirstPurchaseDate).IsRequired(false);

        // AuditableEntity
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100).IsRequired(false);
        builder.Property(c => c.LastModifiedUtc).IsRequired();
        builder.Property(c => c.LastModifiedBy).HasMaxLength(100).IsRequired(false);

        // 5. 🗺️ ربط قائمة الـ Value Objects المغلقة (Addresses Collection)
        builder.OwnsMany(c => c.Addresses, ab =>
        {
            ab.ToTable("CustomerAddresses");
            ab.WithOwner().HasForeignKey("CustomerId");
            ab.HasKey("Id");

            ab.Property(a => a.Street).HasMaxLength(200).IsRequired();
            ab.Property(a => a.City).HasMaxLength(100).IsRequired();
            ab.Property(a => a.State).HasMaxLength(100).IsRequired();
            ab.Property(a => a.ZipCode).HasMaxLength(20).IsRequired();
            ab.Property(a => a.Country).HasMaxLength(100).IsRequired();
        });

        builder.Navigation(c => c.Addresses)
            .Metadata.SetField("_addresses");
        builder.Navigation(c => c.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}