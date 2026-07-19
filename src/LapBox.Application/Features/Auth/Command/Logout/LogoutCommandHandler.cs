using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Auth.Command.Logout;

public sealed class LogoutCommandHandler(
    IIdentityService identityService,
    ILogger<LogoutCommandHandler> logger,
    HybridCache cache) : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand request, CancellationToken ct)
    {
        // 1. التحقق من صحة الـ Guid
        if (request.UserId == Guid.Empty)
        {
            logger.LogWarning("Invalid User ID format during logout: {UserId}", request.UserId);
            return ApplicationErrors.UserIdClaimInvalid;
        }

        // 2. إبطال التوكن من الداتابيز
        var result = await identityService.RevokeRefreshTokenAsync(request.UserId);

        // 3. 💡 هنا يمكنك مسح بيانات المستخدم من الـ Cache إذا كنت تستخدم IMemoryCache أو Redis
        await cache.RemoveByTagAsync($"user_data_{request.UserId}", ct);
        logger.LogInformation("User {UserId} logged out and cache cleared.", request.UserId);
        
        return result;
    }
}