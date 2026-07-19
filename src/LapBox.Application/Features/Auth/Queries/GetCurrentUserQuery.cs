using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : ICachedQuery<Result<AppUserDTO>>
{
    public string CacheKey => $"user_profile_{UserId}";

    // 💡 السر هنا: وضع التاج المرتبط بالمستخدم
    public string[] Tags => [$"user_data_{UserId}"]; 

    public TimeSpan Expiration => TimeSpan.FromMinutes(30);
}