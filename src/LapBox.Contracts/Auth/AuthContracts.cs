namespace LapBox.Contracts.Auth;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresOnUtc,
    DateTime RefreshTokenExpiresOnUtc);

public sealed record TokenResponse(
    string? AccessToken,
    string? RefreshToken,
    DateTime ExpiresOnUtc,
    DateTime RefreshTokenExpiresOnUtc);

public sealed record CurrentUserResponse(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);
