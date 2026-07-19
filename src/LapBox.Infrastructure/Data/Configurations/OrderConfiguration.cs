using LapBox.Domain.Orders;
using LapBox.Domain.Promotions;
using LapBox.Domain.Laptops;
using LapBox.Infrastructure.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LapBox.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // 1. المفتاح الأساسي والـ Clustered Index للأداء المتقدم
        builder.HasKey(o => o.Id).IsClustered(false);
        builder.HasIndex(o => o.OrderDate).IsClustered();

        // 2. الخصائص الأساسية للطلب
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.OrderDate).IsRequired();

        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(o => o.TrackingLabel)
               .HasMaxLength(100)
               .IsRequired(false);

        // 3. 📝 خريطة خصائص الـ AuditableEntity الموروثة للطلب
        builder.Property(o => o.CreatedAtUtc).IsRequired();
        builder.Property(o => o.CreatedBy).HasMaxLength(100).IsRequired(false);
        builder.Property(o => o.LastModifiedUtc).IsRequired();
        builder.Property(o => o.LastModifiedBy).HasMaxLength(100).IsRequired(false);

       // 4. ربط الـ Value Object الخاص بعنوان الشحن بالكامل (تم إضافة الحقول المنسية)
        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(p => p.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
            sa.Property(p => p.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
            sa.Property(p => p.State).HasColumnName("ShippingState").HasMaxLength(100).IsRequired();
            sa.Property(p => p.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired();
            sa.Property(p => p.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
        });

        // 5. دمج الـ OrderItems في بلوك واحد وتصحيح الـ Primary Key والـ Index
        builder.OwnsMany(o => o.OrderItems, oi =>
        {
            oi.ToTable("OrderItems");
            
            // الاعتماد على الـ Id الأساسي الموروث من كلاس Entity
            oi.HasKey(x => x.Id); 

            // عمل Unique Index لمنع تكرار نفس اللابتوب في نفس الأوردر
            oi.HasIndex("OrderId", "LaptopId").IsUnique();

            oi.Property(x => x.Quantity).IsRequired();
            oi.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            oi.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)").IsRequired();

            // العلاقات داخل البلوك الموحد
            oi.HasOne<Laptop>()
              .WithMany()
              .HasForeignKey(x => x.LaptopId)
              .OnDelete(DeleteBehavior.Restrict);

            oi.WithOwner().HasForeignKey("OrderId");
        });

        // 6. ربط الـ Backing Field للـ OrderItems
        builder.Navigation(o => o.OrderItems)
               .Metadata.SetField("_orderItems");
        builder.Navigation(o => o.OrderItems)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // 7. العلاقات الخارجية للـ Aggregate Root
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // تم تثبيت السلوك الصحيح هنا وحذف التكرار المتعارض
        builder.HasOne<Promotion>()
            .WithMany()
            .HasForeignKey(o => o.AppliedPromotionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}