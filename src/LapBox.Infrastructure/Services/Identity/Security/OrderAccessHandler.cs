using System.Security.Claims;
using LapBox.Application.Common.Security;
using LapBox.Infrastructure.Data.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Services.Identity.Security;

public class OrderAccessHandler(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<OrderAccessRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderAccessRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userGuid)) return; 

        // 2. الحصول على الـ OrderId
        var routeData = httpContextAccessor.HttpContext?.Request.RouteValues["orderId"]?.ToString();
        if (!Guid.TryParse(routeData, out var orderId)) return;

        // 3. التحقق من الملكية (مقارنة Guid بـ Guid)
        var isOwner = await dbContext.Orders
            .AnyAsync(o => o.Id == orderId && o.UserId == userGuid);

        if (isOwner)
        {
            context.Succeed(requirement);
            return;
        }

        // 4. استثناء للأدمن
        if (context.User.IsInRole("Admin")) context.Succeed(requirement);
    }
}