using LapBox.Domain.Carts;
using LapBox.Domain.Laptops;
using LapBox.Infrastructure.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

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

        // Owned collection - Items (owned entity, stored in separate CartItems table)
        builder.OwnsMany(c => c.Items, item =>
        {
            item.ToTable("CartItems");

            // Explicit FK: CartItem.CartId → Cart.Id
            item.WithOwner().HasForeignKey("CartId");
            item.HasKey("Id");

            // Map CartItem.CartId to the FK column (no explicit shadow property needed;
            // WithOwner() creates the column and Property() wires it to the CLR property)
            item.Property(i => i.CartId)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CartId");

            item.Property(i => i.LaptopId).IsRequired();
            item.Property(i => i.Quantity).IsRequired();
            item.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();

            // FK: CartItem → Laptop
            item.HasOne<Laptop>()
                .WithMany()
                .HasForeignKey("LaptopId")
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation(c => c.Items)
            .Metadata.SetField("_items");
        builder.Navigation(c => c.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

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
