using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.Login;

public sealed record LoginCommand(string Email, string Password) 
    : IRequest<Result<TokenInfo>>; // بنرجع الـ TokenResponse بتاعك علطول
