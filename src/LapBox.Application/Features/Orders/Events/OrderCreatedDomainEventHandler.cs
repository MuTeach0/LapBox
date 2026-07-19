using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Orders.Events;

namespace LapBox.Application.Features.Orders.Events;

public sealed class OrderCreatedDomainEventHandler(
    ICustomerRepository customerRepository,
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    IOrderNotifier orderNotifier) : IEventHandler<OrderCreatedDomainEvent>
{
    public async Task HandleAsync(OrderCreatedDomainEvent domainEvent, CancellationToken ct)
    {
        // 1. جلب العميل (لو الـ UserId في الـ Event هو نفسه الـ IdentityId، يفضل تستخدم GetByIdentityIdAsync)
        var customer = await customerRepository.GetByIdentityIdAsync(domainEvent.UserId, ct);

        customer?.RegisterSuccessfulPurchase();

        // 2. تفريغ السلة (فك الكومنت واستخدام الفانكشن الجديدة)
        await cartRepository.ClearCartByIdentityIdAsync(domainEvent.UserId, ct);

        // 3. 🚀 حفظ كل التعديلات في خطوة واحدة (تحديث العميل + تفريغ السلة)
        await unitOfWork.SaveChangesAsync(ct);

        // 4. إرسال الإشعار للعميل
        await orderNotifier.NotifyOrderPlacedAsync(domainEvent.OrderId, ct);
    }
}
