using FluentValidation;

namespace LapBox.Application.Features.Auth.Command.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token cannot be empty.")
            .WithErrorCode("Auth.AccessToken.Required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token cannot be empty.")
            .WithErrorCode("Auth.RefreshToken.Required");
    }
}