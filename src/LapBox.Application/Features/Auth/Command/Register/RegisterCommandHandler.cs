using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers.Events;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IPublisher publisher) : IRequestHandler<RegisterCommand, Result<TokenInfo>>
{
    public async Task<Result<TokenInfo>> Handle(RegisterCommand command, CancellationToken ct)
    {
        // 1️⃣ إنشاء المستخدم مع Role = "User"
        // الدور هيتغير لـ "Customer" لما يعمل أول order في CreateOrder
        var createUserResult = await identityService.CreateUserAsync(
            command.Email,
            command.Password,
            command.FirstName,
            command.LastName,
            "User"); // دور "User" - هيتغير لـ "Customer" بعد أول order

        if (createUserResult.IsError) return createUserResult.Errors;

        // 2️⃣ توليد الـ Token فوراً للمستخدم الجديد ليتم تسجيل دخوله تلقائياً
        var userDto = createUserResult.Value;
        await publisher.Publish(new UserRegisteredEvent(
            userDto.UserId, 
            command.Email, 
            command.FirstName, 
            command.LastName), ct);
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