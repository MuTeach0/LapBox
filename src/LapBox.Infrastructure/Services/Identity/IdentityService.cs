using System.Security.Claims;
using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LapBox.Infrastructure.Services.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService) : IIdentityService
{
    public async Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return [];

        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        return await userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string? policyName, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        
        var principal = await userClaimsPrincipalFactory.CreateAsync(user);
        var result = await authorizationService.AuthorizeAsync(principal, policyName!);
        
        return result.Succeeded;
    }

    public async Task<Result<AppUserDTO>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return ApplicationErrors.InvalidCredentials;

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded) return ApplicationErrors.InvalidCredentials;

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        // return new AppUserDTO(user.Id, user.Email!, roles, claims);
        return new AppUserDTO(user.Id, user.Email!, user.FirstName, user.LastName, roles, claims);
    }

    public async Task<Result<AppUserDTO>> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        // return new AppUserDTO(user.Id, user.Email!, roles, claims);
        return new AppUserDTO(user.Id, user.Email!, user.FirstName, user.LastName, roles, claims);
    }

    public async Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user?.UserName;
    }

    public async Task<Result<AppUserDTO>> CreateUserAsync(string email, string password, string firstName, string lastName, string role, CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null) return ApplicationErrors.InvalidCredentials;

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.Failure("User.CreateFailed", errors);
        }

        // Only add role if provided (User = no role, Customer = after first order)
        if (!string.IsNullOrWhiteSpace(role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        var appUser = new AppUserDTO(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            string.IsNullOrWhiteSpace(role) ? new List<string>() : [role], 
            []);

        return appUser;
    }

    public async Task<Result<Success>> RevokeRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user is null) return ApplicationErrors.UserNotFound;

        // 1. تحديث الـ Security Stamp: يبطل أي Access Token و Refresh Token تم إصدارهم مسبقاً
        await userManager.UpdateSecurityStampAsync(user);

        // 💡 ملاحظة: إذا كنت عامل Properties مخصصة في جدول ApplicationUser 
        // مثل (RefreshToken و RefreshTokenExpiryTime)، قم بتصفيرها هنا:
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);
        
        return Result.Success;
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;
            await userManager.UpdateAsync(user);
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        // التحقق: هل المستخدم موجود؟ هل التوكن يطابق المحفوظ؟ هل التوكن لم ينتهِ؟
        return user is not null && 
            user.RefreshToken == refreshToken && 
            user.RefreshTokenExpiryTime > DateTime.UtcNow;
    }

    public async Task<Result<Success>> AddToRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApplicationErrors.UserNotFound;

        var result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.Failure("Identity.AddToRoleFailed", errors);
        }

        return Result.Success;
    }

    public async Task<Result<Success>> RemoveFromRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApplicationErrors.UserNotFound;

        var result = await userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.Failure("Identity.RemoveFromRoleFailed", errors);
        }

        return Result.Success;
    }

    public async Task<Result<Success>> ChangeUserRoleAsync(Guid userId, string oldRole, string newRole, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return ApplicationErrors.UserNotFound;

        // 1. حذف الدور القديم (User)
        var removeResult = await userManager.RemoveFromRoleAsync(user, oldRole);
        if (!removeResult.Succeeded)
        {
            var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
            return Error.Failure("Identity.RemoveOldRoleFailed", errors);
        }

        // 2. إضافة الدور الجديد (Customer)
        var addResult = await userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            return Error.Failure("Identity.AddNewRoleFailed", errors);
        }

        return Result.Success;
    }
}
