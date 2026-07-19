using Microsoft.AspNetCore.Identity;

namespace LapBox.Infrastructure.Services.Identity;

/// <summary>
/// Application User entity for ASP.NET Core Identity - uses Guid as primary key
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
