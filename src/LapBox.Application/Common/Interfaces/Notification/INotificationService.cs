namespace LapBox.Application.Common.Interfaces.Notification;

public interface INotificationService
{
    Task SendEmailAsync(string to, string message, CancellationToken cancellationToken = default);
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task NotifyUsersPriceDroppedAsync(Guid laptopId, decimal newPrice, CancellationToken cancellationToken = default);
    
}