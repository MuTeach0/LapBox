using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(
    Guid OrderId, 
    Guid CurrentUserId, 
    string CurrentUserRole) : ICachedQuery<Result<OrderDetailsDTO>>
{
    // أضفنا الـ UserId في الـ Key عشان الكاش يبقى مخصص لكل مستخدم وميحدثش تداخل بيانات
    public string CacheKey => $"order_{OrderId}_{CurrentUserId}"; 
    public string[] Tags => ["orders"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}