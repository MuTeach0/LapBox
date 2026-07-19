using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken)
    : IRequest<Result<TokenInfo>>;