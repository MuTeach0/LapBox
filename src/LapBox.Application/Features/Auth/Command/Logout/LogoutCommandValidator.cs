using FluentValidation;

namespace LapBox.Application.Features.Auth.Command.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required to perform logout.")
            .Must(BeAValidGuid).WithMessage("User ID must be a valid GUID format.");
    }

    private static bool BeAValidGuid(Guid userId) => userId != Guid.Empty;
}