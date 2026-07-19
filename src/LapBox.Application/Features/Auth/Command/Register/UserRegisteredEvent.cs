using MediatR;

namespace LapBox.Application.Features.Auth.Command.Register;

public sealed record UserRegisteredEvent(
    Guid UserId, 
    string Email, 
    string FirstName, 
    string LastName) : INotification;