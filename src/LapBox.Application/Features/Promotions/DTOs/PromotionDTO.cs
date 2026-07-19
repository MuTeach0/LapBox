namespace  LapBox.Application.Features.Promotions.DTOs;

public sealed record PromotionDTO(Guid Id, string Code, decimal DiscountPercentage, DateTimeOffset EndDate);
