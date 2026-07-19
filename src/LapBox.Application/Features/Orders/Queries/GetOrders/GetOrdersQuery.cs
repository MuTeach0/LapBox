using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    int Page,
    int PageSize,
    Guid CurrentUserId,    // معرف الـ Admin الحالي للـ Logging
    string CurrentUserRole // دور المستخدم الحالي للتحقق من الصلاحية
) : ICachedQuery<Result<PaginatedList<OrderSummaryDTO>>>
{
    // كاش موحد لكل الإدارة، وبيطهر تلقائياً أول ما أي طلب حالته تتغير 
    // لأننا رابطينه بـ Tag اسمه "orders"
    public string CacheKey => "all_orders_dashboard";
    public string[] Tags => ["orders"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(5); // وقت أقل لأن داتا الإدارة بتتحدث بسرعة
}