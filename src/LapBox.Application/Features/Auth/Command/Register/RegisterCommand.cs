using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.Register;
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : IRequest<Result<TokenInfo>>;