using Microsoft.EntityFrameworkCore;
using LapBox.Domain.Orders;
using LapBox.Domain.Customers;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Carts;
using LapBox.Domain.Catalog;
using LapBox.Domain.Laptops;
using LapBox.Domain.Promotions;
using LapBox.Domain.Reviews;
using LapBox.Domain.Billing;
using LapBox.Domain.StockReservations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LapBox.Infrastructure.Services.Identity;

namespace LapBox.Infrastructure.Data.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Laptop> Laptops => Set<Laptop>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var efTransaction = await Database.BeginTransactionAsync(cancellationToken);
        return new EfCoreTransactionAdapter(efTransaction);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تطبيق كل إعدادات الجداول تلقائياً
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}