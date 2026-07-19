using LapBox.Application.Common.Interfaces.Identity;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Common.Behaviors;


public class LoggingBehavior<TRequest>(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id;
        string? userName = null;

        if (userId.HasValue)
        {
            userName = await _identityService.GetUserNameAsync(userId.Value);
        }

        _logger.LogInformation(
            "Request: {Name} {@UserId} {@UserName} {@Request}", requestName, userId, userName, request);
    }
}