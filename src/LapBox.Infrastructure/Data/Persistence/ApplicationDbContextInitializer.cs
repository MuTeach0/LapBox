using LapBox.Domain.Catalog;
using Microsoft.AspNetCore.Identity;
using LapBox.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LapBox.Infrastructure.Data.Persistence;

public class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger, 
    AppDbContext context,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task InitializeAsync()
    {
        try
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Roles (الأساسية في النظام)
        var roles = new[] { "Admin", "User", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // 2. Seed The Only Admin User (صاحب المنصة)
        var adminEmail = "admin@lapbox.local";
        var adminPassword = "AdminPassword123!"; // ضع باسورد قوية
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is null)
        {
            var adminUser = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                FirstName = "System",
                LastName = "Admin",
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        /*
        {
            "email": "admin@lapbox.local",
            "password": "AdminPassword123!"
        }

        {
            "brandId": "C6EBBF0F-2358-4AC0-8FC1-A8A8D2B7EC53",
            "categoryId": "C0F93E3F-DF90-45C8-A935-BC73BBFCC53F",
            "name": "LAPTOP",
            "sku": "hp elit 840 i7-8850U/8/256/intel",
            "description": "Business laptop",
            "basePrice": "8500.00",
            "inventoryQuantity": "8",
            "processor": "i7-8850U",
            "ram": "8",
            "storage": "256",
            "screenSize": "15.6",
            "graphicsCard": "intel"
        }
        */

        // 1. Seed Brands
        if (!await context.Brands.AnyAsync())
        {
            var brands = new[]
            {
                Brand.Create("Apple", "Premium laptops from Apple", null),
                Brand.Create("Dell", "High performance business and gaming laptops", null),
                Brand.Create("HP", "Reliable laptops for work and home", null),
                Brand.Create("Lenovo", "Versatile laptops for every user", null)
            };

            foreach (var result in brands)
            {
                if (result.IsSuccess)
                {
                    context.Brands.Add(result.Value);
                }
            }
            await context.SaveChangesAsync();
        }

        // 2. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            // ملاحظة: لو الـ Category فيها ميثود Create، استخدمها بنفس الطريقة
            // لو لسه معملتش Create للـ Category، ممكن تعملها بنفس نمط الـ Brand
            var categories = new[]
            {
                Category.Create("Gaming", "High-end gaming laptops"),
                Category.Create("Ultrabook", "Thin and light laptops for mobility"),
                Category.Create("Business", "Professional laptops for office work"),
                Category.Create("Workstation", "Powerful machines for professionals")
            };

            foreach (var result in categories)
            {
                if (result.IsSuccess)
                {
                    context.Categories.Add(result.Value);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}

public static class InitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        await initializer.InitializeAsync();
        await initializer.SeedAsync();
    }
}
