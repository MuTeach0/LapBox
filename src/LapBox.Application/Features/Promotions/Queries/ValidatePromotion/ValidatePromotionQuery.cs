using LapBox.Application.Features.Promotions.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Promotions.Queries.ValidatePromotion;

public sealed record ValidatePromotionQuery(string Code) : IRequest<Result<PromotionDTO>>;