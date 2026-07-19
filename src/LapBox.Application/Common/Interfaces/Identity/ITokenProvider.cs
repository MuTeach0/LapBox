using System.Security.Claims;
using LapBox.Application.Features.Auth;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Common.Interfaces.Identity;

public interface ITokenProvider
{
    Task<Result<TokenInfo>> GenerateJwtTokenAsync(AppUserDTO user, CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}