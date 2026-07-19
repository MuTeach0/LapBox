using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<IList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<bool> AuthorizeAsync(Guid userId, string? policyName, CancellationToken ct = default);
    Task<Result<AppUserDTO>> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<Result<AppUserDTO>> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct = default);
    Task<Result<AppUserDTO>> CreateUserAsync(string email, string password, string firstName, string lastName, string role, CancellationToken ct = default);
    Task<Result<Success>> RevokeRefreshTokenAsync(Guid userId, CancellationToken ct = default);
    Task<Result<Success>> AddToRoleAsync(Guid userId, string role, CancellationToken ct = default);
    
    // دوال الترقية وإدارة الصلاحيات
    Task<Result<Success>> RemoveFromRoleAsync(Guid userId, string role, CancellationToken ct = default);
    Task<Result<Success>> ChangeUserRoleAsync(Guid userId, string oldRole, string newRole, CancellationToken ct = default);
    
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime, CancellationToken ct = default);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken ct = default);
}