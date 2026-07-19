using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Interfaces.Policies;

namespace LapBox.Infrastructure.Services;

public sealed class PromotionPolicy(
    IPromotionRepository promotionRepository,
    IOrderRepository orderRepository) : IPromotionPolicy
{
    public async Task<bool> IsValidForCustomerAsync(
        Guid promotionId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var promotion = await promotionRepository.GetByIdAsync(promotionId, ct);
        if (promotion is null || !promotion.IsActive) return false;

        // لو الكود ده مستخدم قبل كده من العميل ده → مش صالح
        var customerOrders = await orderRepository.GetByUserIdAsync(customerId, ct);
        if (customerOrders.Any(o => o.AppliedPromotionId == promotionId)) return false;

        return true;
    }
}
