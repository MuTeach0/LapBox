using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Domain.Common.Results;
using System.Security.Claims;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Result<TokenInfo>>
{
    public async Task<Result<TokenInfo>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var principal = tokenProvider.GetPrincipalFromExpiredToken(command.AccessToken);
        if (principal is null) return ApplicationErrors.ExpiredAccessTokenInvalid;

        var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            return ApplicationErrors.UserIdClaimInvalid;

        var isValid = await identityService.ValidateRefreshTokenAsync(userId, command.RefreshToken);
        if (!isValid) return ApplicationErrors.InvalidRefreshToken; // أو خطأ مناسب

        var userResult = await identityService.GetUserByIdAsync(userId);
        if (userResult.IsError) return userResult.Errors;

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, ct);

        if (tokenResult.IsSuccess)
        {
            await identityService.UpdateRefreshTokenAsync(
                userId,
                tokenResult.Value.RefreshToken!,
                tokenResult.Value.RefreshTokenExpiresOnUtc);
        }
        return tokenResult;
    }
}