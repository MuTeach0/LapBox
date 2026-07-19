using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Orders.Queries.GetOrdersByUserId;

public sealed record GetOrdersByUserIdQuery(
    Guid UserId,           // المعرف المراد جلب طلباته (الهدف)
    Guid CurrentUserId,    // معرف المستخدم الحالي من الـ Token
    string CurrentUserRole // دور المستخدم الحالي من الـ Token
) : ICachedQuery<Result<List<OrderSummaryDTO>>>
{
    // الـ CacheKey هيفضل معتمد على الـ UserId لأن البيانات تخص هذا المستخدم فقط،
    // لكن الـ Handler هيمنع أي حد غريب من الوصول للكاش ده من الأساس.
    public string CacheKey => $"orders_user_{UserId}";
    public string[] Tags => ["orders"];
    public TimeSpan Expiration => TimeSpan.FromHours(1);
}