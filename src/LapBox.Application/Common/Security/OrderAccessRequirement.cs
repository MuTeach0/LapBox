using Microsoft.AspNetCore.Authorization;

namespace LapBox.Application.Common.Security;

public record OrderAccessRequirement : IAuthorizationRequirement;