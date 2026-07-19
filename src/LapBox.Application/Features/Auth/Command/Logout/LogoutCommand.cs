using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Auth.Command.Logout;

public sealed record LogoutCommand(Guid UserId) : IRequest<Result<Success>>;