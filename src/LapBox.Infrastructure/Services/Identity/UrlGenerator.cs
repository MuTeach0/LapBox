using LapBox.Application.Common.Interfaces.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LapBox.Infrastructure.Services.Identity;

public class UrlGenerator(LinkGenerator linkGenerator, IHttpContextAccessor httpContext) : IUrlGenerator
{
    public string GenerateResetPasswordUrl(string token)
    {
        var context = httpContext.HttpContext ?? throw new InvalidOperationException("HTTP context is not available.");
        return linkGenerator.GetUriByAction(context, "ResetPassword", "Auth", new { token })!;
    }
}