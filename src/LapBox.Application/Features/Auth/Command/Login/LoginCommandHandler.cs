using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider) : IRequestHandler<LoginCommand, Result<TokenInfo>>
{
    public async Task<Result<TokenInfo>> Handle(LoginCommand command, CancellationToken ct)
    {
        // 1️⃣ التحقق من الهوية وصحة الحساب عبر الـ IdentityService
        var authResult = await identityService.AuthenticateAsync(command.Email, command.Password);
        if (authResult.IsError) return authResult.Errors;

        // 2️⃣ توليد الـ Token باستخدام الـ AppUserDTO المسترجع
        var userDto = authResult.Value;
        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userDto, ct);

        if (tokenResult.IsSuccess)
        {
            await identityService.UpdateRefreshTokenAsync(
                userDto.UserId,
                tokenResult.Value.RefreshToken!,
                tokenResult.Value.RefreshTokenExpiresOnUtc);
        }
        return tokenResult;
    }
}