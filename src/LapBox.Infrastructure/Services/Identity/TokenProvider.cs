using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Features.Auth;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;
using Microsoft.IdentityModel.Tokens;

namespace LapBox.Infrastructure.Services.Identity;

public sealed class TokenProvider(JwtSettings jwtSettings, RefreshTokenSettings refreshTokenSettings) : ITokenProvider
{
    public Task<Result<TokenInfo>> GenerateJwtTokenAsync(AppUserDTO user, CancellationToken ct = default)
    {
        var accessToken = GenerateAccessToken(user, ct);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpires = DateTime.UtcNow.AddDays(refreshTokenSettings.ExpiryInDays);
        var response = new TokenInfo
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes),
            RefreshTokenExpiresOnUtc = refreshTokenExpires
        };

        return Task.FromResult<Result<TokenInfo>>(response);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // Allow expired tokens for refresh
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }

    private string GenerateAccessToken(AppUserDTO user, CancellationToken ct = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        foreach (var claim in user.Claims)
        {
            claims.Add(claim);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryInMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

public sealed class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; } = 60;
}

public sealed class RefreshTokenSettings
{
    public int ExpiryInDays { get; set; } = 7;
}
