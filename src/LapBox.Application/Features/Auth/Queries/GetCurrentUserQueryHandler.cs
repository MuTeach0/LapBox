using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Auth.Queries;

public sealed class GetCurrentUserQueryHandler(
    IIdentityService identityService,
    IUser currentUser,
    ILogger<GetCurrentUserQueryHandler> logger) : IRequestHandler<GetCurrentUserQuery, Result<AppUserDTO>>
{
    public async Task<Result<AppUserDTO>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        if (currentUser.Id is null)
        {
            logger.LogWarning("Attempt to access current user profile without a valid UserId claim.");
            return ApplicationErrors.UserIdClaimInvalid;
        }

        if (currentUser.Id != request.UserId)
        {
            logger.LogWarning("Security Alert: User {Current} tried to access profile of {Requested}", currentUser.Id, request.UserId);
            return ApplicationErrors.Unauthorized; 
        }

        // var userId = currentUser.Id.Value;
        var userResult = await identityService.GetUserByIdAsync(request.UserId);
        
        if (userResult.IsError)
        {
            logger.LogError("User profile fetch failed for UserId: {UserId}. Error: {ErrorDetails}", 
                request.UserId, userResult.TopError.Description);
                
            return ApplicationErrors.UserNotFound;
        }

        return userResult.Value;
    }
}