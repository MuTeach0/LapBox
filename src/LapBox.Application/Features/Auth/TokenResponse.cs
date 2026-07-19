namespace LapBox.Application.Features.Auth;

/// <summary>
/// Internal token data used by MediatR handlers. Not exposed directly via the API.
/// The public-facing contract is LapBox.Contracts.Auth.TokenResponse.
/// </summary>
public class TokenInfo
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime RefreshTokenExpiresOnUtc { get; set; }
}
