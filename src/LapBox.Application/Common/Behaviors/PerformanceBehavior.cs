using System.Diagnostics;
using LapBox.Application.Common.Interfaces.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.Id;
            string? userName = null;

            if (userId.HasValue)
            {
                userName = await _identityService.GetUserNameAsync(userId.Value);
            }

            _logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}", requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}