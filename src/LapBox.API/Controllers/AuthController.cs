using System.Security.Claims;
using LapBox.Application.Features.Auth.Command.Login;
using LapBox.Application.Features.Auth.Command.Logout;
using LapBox.Application.Features.Auth.Command.RefreshToken;
using LapBox.Application.Features.Auth.Command.Register;
using LapBox.Application.Features.Auth.DTOs;
using LapBox.Application.Features.Auth.Queries;
using LapBox.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapBox.API.Controllers;

[Route("api/[controller]")]
public sealed class AuthController(ISender mediator) : ApiController 
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Register a new customer account.")]
    [EndpointDescription("Creates a new user account with the provided details and returns a JWT token pair.")]
    [EndpointName("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password);
        var result = await mediator.Send(command, ct);

        return result.Match(
            value => value is null
                ? NoContent()
                : CreatedAtAction(nameof(Login), new AuthResponse(
                    value.AccessToken!,
                    value.RefreshToken!,
                    value.ExpiresOnUtc,
                    value.RefreshTokenExpiresOnUtc)),
            errors => Problem(errors)); // سيتم توجيه الخطأ تلقائياً للـ Base Controller
    }

    [HttpPost("login", Name = nameof(Login))]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Login with email and password.")]
    [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
    [EndpointName("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await mediator.Send(command, ct);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Refresh access token using a valid refresh token.")]
    [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
    [EndpointName("RefreshToken")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await mediator.Send(command, ct);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Logout the current user.")]
    [EndpointDescription("Invalidates the current user's tokens on the client side.")]
    [EndpointName("Logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        // 1. استخراج الـ Id الخاص بالمستخدم من التوكن الحالي
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // 2. إرسال أمر لإبطال الـ Refresh Token في الداتابيز
        var command = new LogoutCommand(Guid.Parse(userId)); // (ستحتاج لإنشاء هذا الـ Command في طبقة Application)
        var result = await mediator.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AppUserDTO), StatusCodes.Status200OK)] // AppUserDto هو كلاس يحتوي بيانات اليوزر
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Gets the current authenticated user's info.")]
    [EndpointName("GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        // 1. استخراج الـ Id الخاص بالمستخدم من التوكن الحالي
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 2. تحقق أمني: التأكد من أن الـ ID موجود وصحيح (Robustness)
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // 3. استدعاء الـ Query
        var query = new GetCurrentUserQuery(userId);
        var result = await mediator.Send(query, ct);

        // 4. إرجاع النتيجة
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
