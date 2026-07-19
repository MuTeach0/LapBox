using LapBox.Domain.Billing;
using LapBox.Domain.Customers;
using LapBox.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id).IsClustered(false);
        builder.HasIndex(i => i.OrderId).IsUnique();

        builder.Property(i => i.OrderId).IsRequired();
        builder.Property(i => i.CustomerId).IsRequired();

        builder.Property(i => i.SubTotal).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0);
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.IssuedAtUtc).IsRequired();
        builder.Property(i => i.PaidDateUtc).IsRequired(false);

        // Owned collection - LineItems
        builder.OwnsMany(i => i.LineItems, li =>
        {
            li.ToTable("InvoiceLineItems");
            li.WithOwner().HasForeignKey("InvoiceId");
            li.HasKey(x => x.Id);

            li.Property(l => l.InvoiceId).IsRequired();
            li.Property(l => l.LineNumber).IsRequired();
            li.Property(l => l.Description).HasMaxLength(500).IsRequired();
            li.Property(l => l.Quantity).IsRequired();
            li.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            li.Property(l => l.LineTotal).HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Navigation(i => i.LineItems)
            .Metadata.SetField("_lineItems");
        builder.Navigation(i => i.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Enforce FK relationships: Invoice -> Order (1:1) and Invoice -> Customer (many)
        builder.HasOne<Order>()
            .WithOne()
            .HasForeignKey<Invoice>(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuditableEntity
        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(100);
        builder.Property(i => i.LastModifiedUtc).IsRequired();
        builder.Property(i => i.LastModifiedBy).HasMaxLength(100);
    }
}
