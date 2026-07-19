using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Promotions.Command.CreatePromotion;
public sealed record CreatePromotionCommand(
    string Code,
    decimal DiscountPercentage,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate) : IRequest<Result<Guid>>;