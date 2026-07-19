using LapBox.Application.Common.Interfaces.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LapBox.Infrastructure.Services;

/// <summary>
/// Resolves the current user ID from the HTTP ClaimsPrincipal.
/// Registered as Scoped so it picks up the current request context.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    public Guid? Id
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                              ?? httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            return userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var id)
                ? id
                : null;
        }
    }

    // جلب كل الأدوار من التوكن (JWT)
    public List<string> Roles =>
        httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

    // دالة عامة للتحقق من أي دور بناءً على التوكن
    public bool IsInRole(string role) =>
        httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    // Property جاهزة للتحقق من دور Customer تحديداً
    public bool IsCustomer => IsInRole("Customer");
}
