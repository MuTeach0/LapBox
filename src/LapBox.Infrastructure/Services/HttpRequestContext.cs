using LapBox.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace LapBox.Infrastructure.Services;

/// <summary>
/// HTTP-level request context carrying user info and client IP.
/// </summary>
public sealed class HttpRequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public Guid? UserId => httpContextAccessor.HttpContext?.Items["UserId"] as Guid?;
    public string? UserEmail => httpContextAccessor.HttpContext?.Items["UserEmail"] as string;
    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
