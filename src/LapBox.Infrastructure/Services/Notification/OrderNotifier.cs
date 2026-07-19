using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Application.Common.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace LapBox.Infrastructure.Services.Notification;

public sealed class OrderNotifier(
    INotificationService notificationService,
    IOrderRepository orderRepository,
    ICustomerRepository userRepository, // 1. أضف مستودع المستخدمين هنا
    ILogger<OrderNotifier> logger) : IOrderNotifier
{
    public async Task NotifyOrderCancelledAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) return;

        // 2. جلب بيانات المستخدم بناءً على الـ CustomerId
        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user is null) return;

        logger.LogInformation("Order {OrderId} has been cancelled.", orderId);
        string message = $"عذراً! تم إلغاء طلبك رقم {orderId}.";
        await notificationService.SendEmailAsync(user.Email!, message, ct); // استخدم إيميل المستخدم
    }

    public async Task NotifyOrderDispatchedAsync(Guid orderId, string trackingLabel, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) return;
        
        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null) return;

        logger.LogInformation("Order {OrderId} has been dispatched with tracking: {TrackingLabel}", orderId, trackingLabel);
        string message = $"مرحباً! طلبك رقم {orderId} تم شحنه وجاري التوصيل. رقم التتبع الخاص بك: {trackingLabel}";
        
        await notificationService.SendEmailAsync(user.Email!, message, ct);
        
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            await notificationService.SendSmsAsync(user.PhoneNumber, message, ct);
        }
    }

    public async Task NotifyOrderDeliveredAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) return;

        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null) return;

        // 1. تعديل اللوج ليعبر عن التوصيل
        logger.LogInformation("Order {OrderId} has been successfully delivered.", orderId);

        // 2. تعديل نص الرسالة
        string message = $"مرحباً! تم توصيل طلبك رقم {orderId} بنجاح. نتمنى لك تجربة مميزة مع LapBox!";

        await notificationService.SendEmailAsync(user.Email!, message, ct);

        // 3. إضافة شرط التأكد من رقم الهاتف لتجنب الـ Exceptions
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            await notificationService.SendSmsAsync(user.PhoneNumber, message, ct);
    }

    public async Task NotifyOrderPlacedAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) return;

        var user = await userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null) return;

        logger.LogInformation("Order {OrderId} has been placed.", orderId);
        string message = $"شكراً لتسوقك من LapBox! تم استلام طلبك رقم {orderId} وجاري تجهيزه.";
        await notificationService.SendEmailAsync(user.Email!, message, ct);
    }
}