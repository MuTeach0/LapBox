using LapBox.Domain.Common;

namespace LapBox.Domain.Promotions.Events;

public record PromotionCreatedEvent(
    Guid PromotionId,
    string Code,
    decimal DiscountPercentage) : DomainEvent;