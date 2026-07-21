using LapBox.Domain.Carts;
using LapBox.Domain.Laptops;
using LapBox.Infrastructure.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

/// <summary>
/// CartItem is mapped as a regular entity (not owned).
/// This avoids EF Core owned-entity quirks where new owned instances could be
/// treated as UPDATE instead of INSERT on SaveChanges.
/// Cart → CartItem is a standard one-to-many relationship.
/// </summary>
public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id).IsClustered(false);
        builder.HasIndex(c => c.IdentityId).IsUnique();
        builder.Property(c => c.IdentityId).IsRequired();
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // FK: Cart → ApplicationUser (Identity)
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Cart>(c => c.IdentityId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditableEntity fields
        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.LastModifiedUtc).IsRequired();
        builder.Property(c => c.LastModifiedBy).HasMaxLength(100);
    }
}

/// <summary>
/// CartItem is a regular entity. EF Core has full change-tracking control,
/// so new instances are correctly INSERTed (not UPDATEd as with owned entities).
/// </summary>
public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id).IsClustered(false);

        builder.Property(i => i.CartId).IsRequired();
        builder.Property(i => i.LaptopId).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // FK: CartItem → Cart
        builder.HasOne<Cart>()
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: CartItem → Laptop
        builder.HasOne<Laptop>()
            .WithMany()
            .HasForeignKey(i => i.LaptopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
