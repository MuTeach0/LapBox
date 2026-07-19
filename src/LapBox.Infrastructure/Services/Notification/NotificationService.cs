using LapBox.Application.Common.Interfaces.Notification;
using Microsoft.Extensions.Logging;

namespace LapBox.Infrastructure.Services.Notification;
public sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public async Task NotifyUsersPriceDroppedAsync(Guid laptopId, decimal newPrice, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Notifying subscribers for laptop {LaptopId}. New Price: {Price:C}", laptopId, newPrice);
        
        // هنا مستقبلاً لما تجيب إيميلات الناس، هتنادي على الدالة اللي تحت وتبعت الرسالة الصح:
        // await SendEmailAsync(userEmail, $"لحق! سعر اللابتوب نزل لـ {newPrice:C}");
        
        await Task.CompletedTask;
    }

    public async Task SendEmailAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        var at = to.IndexOf('@');
        var maskedEmail = at > 1
            ? to[0] + new string('*', at - 2) + to[at - 1] + to[at..]
            : "*****";

        // الآن الرسالة بتطبع حسب اللي مبعوت للدالة فعلياً
        logger.LogInformation("[Email] To: {Email} | Message: {Message}", maskedEmail, message);

        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        var masked = phoneNumber.Length >= 4
            ? new string('*', phoneNumber.Length - 4) + phoneNumber[^4..]
            : "****";

        logger.LogInformation("[SMS] To: {Phone} | Message: {Message}", masked, message);

        await Task.CompletedTask;
    }
}