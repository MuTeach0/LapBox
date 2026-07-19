using LapBox.Domain.Laptops;
using LapBox.Domain.Reviews;
using LapBox.Infrastructure.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id).IsClustered(false);
        builder.HasIndex(r => r.LaptopId);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.LaptopId, r.UserId }).IsUnique();

        builder.Property(r => r.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        // Owned Value Object - Rating
        builder.OwnsOne(r => r.Rating, rt =>
        {
            rt.Property(rating => rating.Value)
                .IsRequired()
                .HasColumnName("Rating");
        });

        // AuditableEntity
        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedUtc).IsRequired();
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);

        // Foreign keys to enforce referential integrity
        builder.HasOne<Laptop>()
            .WithMany()
            .HasForeignKey(r => r.LaptopId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserId refers to ApplicationUser (AspNetUsers) - enforce FK to identity
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
