namespace LapBox.Infrastructure.Settings;

public class AppSettings
{
    // إعدادات المتجر العامة
    public string StoreCurrency { get; set; } = "USD";
    public decimal TaxRate { get; set; } = 0.15m;
    public decimal FreeShippingThreshold { get; set; } = 500m;

    // إعدادات التخزين (Caching)
    public int LocalCacheExpirationInMins { get; set; } = 30;
    public int DistributedCacheExpirationMins { get; set; } = 60;

    // إعدادات الـ Pagination
    public int DefaultPageNumber { get; set; } = 1;
    public int DefaultPageSize { get; set; } = 10;

    // إعدادات الـ CORS
    public string CorsPolicyName { get; set; } = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public int ReservationCleanupIntervalMinutes { get; set; } = 1; // القيمة الافتراضية
}