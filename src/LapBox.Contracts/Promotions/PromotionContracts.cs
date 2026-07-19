namespace LapBox.Contracts.Promotions;

public sealed record PromotionResponse(
    Guid Id,
    string Code,
    decimal DiscountPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool IsActive);

public sealed record CreatePromotionRequest(
    string Code,
    decimal DiscountPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public sealed record ValidatePromotionRequest(
    string Code,
    decimal OrderSubTotal);

public sealed record ValidatePromotionResponse(
    bool IsValid,
    string Code,
    decimal DiscountAmount,
    decimal FinalSubTotal,
    string? ErrorMessage);
