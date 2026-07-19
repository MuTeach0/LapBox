using System.Security.Claims;

namespace LapBox.Application.Features.Auth.DTOs;

public sealed record AppUserDTO(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IList<string> Roles,
    IList<Claim> Claims);