using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Interfaces;
using LapBox.Application.Common.Interfaces.Policies;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Common.Interfaces.Services;
using LapBox.Application.Common.Interfaces.Storage;
using LapBox.Infrastructure.BackgroundServices;
using LapBox.Infrastructure.Data.Persistence;
using LapBox.Infrastructure.Data.Persistence.Repositories;
using LapBox.Infrastructure.Services;
using LapBox.Infrastructure.Services.Identity;
using LapBox.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using LapBox.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using LapBox.Infrastructure.Services.Identity.Security;
using LapBox.Infrastructure.Settings;
using Microsoft.Extensions.Caching.Hybrid;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Infrastructure.Services.Notification;
using LapBox.Infrastructure.Services.Pdf;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- [NEW] 0. Bind Configuration Settings ---
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() 
            ?? throw new InvalidOperationException("JWT Settings missing");

        services.AddSingleton(jwtSettings);
        services.AddSingleton(configuration.GetSection("RefreshTokenSettings").Get<RefreshTokenSettings>() ?? new RefreshTokenSettings());

        // --- [NEW] 0.1 Add HttpContextAccessor ---
        services.AddHttpContextAccessor();

        // 1. Modern Time Provider for Testing
        services.AddSingleton(TimeProvider.System);

        // 2. Interceptors for Auditing & Domain Events
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>(); // 👈 أضف هذا السطر

        // 3. DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();

        // 4. Identity (IdentityCore is lighter for APIs)
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // 5. Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer, // استخدمنا الاوبجكت اللي عملناله Bind
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // 5.1 Authorization Policies
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole(nameof(LapBox.Domain.Common.Role.Admin)));

        // 6. Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ILaptopRepository, LaptopRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IStockReservationRepository, StockReservationRepository>();
        // جوه ملف تسجيل الخدمات (مثلاً AddInfrastructure أو Program.cs)

        // 1. تسجيل خدمة إشعارات الطلبات
        services.AddScoped<IOrderNotifier, OrderNotifier>();

        // 2. تسجيل خدمة الإشعارات العامة
        services.AddScoped<INotificationService, NotificationService>();

        // 3. تسجيل مولد ملفات الـ PDF للـ Invoice
        services.AddTransient<IInvoicePdfGenerator, InvoicePdfGenerator>();

        // 7. Identity & Business Services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IOrderCheckService, OrderCheckService>();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<IRequestContext, HttpRequestContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUrlGenerator, UrlGenerator>();
        
        // --- [NEW] 7.1 Authorization Handlers ---
        services.AddScoped<IAuthorizationHandler, OrderAccessHandler>();

        // 8. Policies & Background Services
        services.AddScoped<IOrderPolicy, OrderPolicy>();
        services.AddScoped<IPromotionPolicy, PromotionPolicy>();
        services.AddScoped<ILaptopPolicy, LaptopPolicy>();
        services.AddHostedService<StockReservationCleanupService>();
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10), // Set a default expiration time for cache entries
            LocalCacheExpiration = TimeSpan.FromMinutes(30) // Set a default expiration time for local cache entries
        });

        // 9. Utilities
        services.AddScoped<IFileService>(sp =>
            new LocalFileService(sp.GetRequiredService<IWebHostEnvironment>()?.ContentRootPath ?? "wwwroot"));

        return services;
    }
}