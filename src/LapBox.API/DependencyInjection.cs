using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;

using LapBox.API.Infrastructure;
using LapBox.API.OpenApi.Transformers;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Infrastructure.Services;
using LapBox.Infrastructure.Settings;

using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using OpenTelemetry.Metrics;


namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services.AddCustomProblemDetails()
                .AddCustomApiVersioning()
                .AddApiDocumentation()
                .AddExceptionHandling()
                .AddControllerWithJsonConfiguration()
                .AddValidation()
                .AddConfiguredCors(configuration)
                .AddIdentityInfrastructure()
                .AddAppRateLimiting()
                .AddAppOutputCaching()
                .AddAppOpenTelemetry();

        return services;
    }

    public static IServiceCollection AddAppOutputCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; // 100 mb
            options.AddBasePolicy(policy =>
                policy.Expire(TimeSpan.FromSeconds(60)));
        });

        return services;
    }
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
        .ConfigureResource(res => res.AddService("overservice"))
        .WithTracing(static tracing =>
        {
            tracing.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            tracing.AddOtlpExporter();
        }).
        WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            metrics.AddOtlpExporter().
            AddPrometheusExporter(); // /metrics
        });

        return services;
    }

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
         services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("SlidingWindow", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.QueueLimit = 10;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.AutoReplenishment = true;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = (context) =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        });

        return services;
    }

    public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
    {
         services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1"];

        foreach (var version in versions)
        {
            services.AddOpenApi(version, options =>
            {
                // Versioning config
                options.AddDocumentTransformer<VersionInfoTransformer>();

                // Security Scheme config
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

                // Security Operation config
                options.AddOperationTransformer<BearerSecurityOperationTransformer>();
            });
        }

        return services;
    }

    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
    public static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options => options
            .JsonSerializerOptions
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services) => services;
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        var allowedOrigins = appSettings.AllowedOrigins?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray() ?? Array.Empty<string>();

        services.AddCors(options => options.AddPolicy(appSettings.CorsPolicyName, policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins);
            }
            else
            {
                policy.AllowAnyOrigin();
            }

            policy.AllowAnyHeader()
                .AllowAnyMethod();
        }));

        return services;
    }

    public static IApplicationBuilder UseCoreMiddlewares(this IApplicationBuilder app, IConfiguration configuration)
    {
        // 1. Exception handling should be FIRST to catch all errors
        app.UseExceptionHandler();

        // 2. Status code pages for handling HTTP status codes
        app.UseStatusCodePages();

        // 3. HTTPS redirection (before any other middleware that might generate URLs)
        app.UseHttpsRedirection();

        // 4. Serilog request logging (early to log all requests)
        app.UseSerilogRequestLogging();

        // 5. CORS (before authentication/authorization)
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        app.UseCors(appSettings.CorsPolicyName);

        // 6. Rate limiting (before authentication to protect auth endpoints)
        app.UseRateLimiter();

        // 7. Authentication (must come before authorization)
        app.UseAuthentication();

        // 8. Authorization (must come after authentication)
        app.UseAuthorization();

        // 9. Output caching (after auth to cache based on user context)
        app.UseOutputCache();

        return app;
    }
}