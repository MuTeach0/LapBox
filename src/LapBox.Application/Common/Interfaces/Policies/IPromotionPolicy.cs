namespace LapBox.Application.Common.Interfaces.Policies;

public interface IPromotionPolicy
{
    Task<bool> IsValidForCustomerAsync(Guid promotionId, Guid customerId, CancellationToken ct = default);
}