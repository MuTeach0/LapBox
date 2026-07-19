using System.Security.Claims;

using LapBox.Application.Common.Interfaces.Identity;

namespace LapBox.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
   public Guid? Id => httpContextAccessor.HttpContext?.User?.FindFirstValue
    (ClaimTypes.NameIdentifier) is string id ? Guid.Parse(id) : null;

    public List<string> Roles =>
        httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

    // التحقق السريع من امتلاك المستخدم لدور Customer
    public bool IsCustomer => IsInRole("Customer");

    // التحقق من امتلاك المستخدم لأي دور يتم تمريره كـ Parameter
    public bool IsInRole(string role) =>
        httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}