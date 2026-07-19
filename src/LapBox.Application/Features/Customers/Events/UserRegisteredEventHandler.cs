using LapBox.Application.Features.Auth.Command.Register;
using MediatR;

namespace LapBox.Application.Features.Customers.Events;

/// <summary>
/// Handler for UserRegisteredEvent.
/// 
/// NOTE: User registration no longer creates a Customer record.
/// Customer is only created when the user makes their first order (CreateOrder).
/// This handler is kept for future extensibility (e.g., sending welcome emails).
/// </summary>
public sealed class UserRegisteredEventHandler
    : INotificationHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent notification, CancellationToken ct)
    {
        // TODO: Future extensibility - send welcome email, analytics, etc.
        // Customer record is NOT created here - only in CreateOrder for first-time buyers
        return Task.CompletedTask;
    }
}