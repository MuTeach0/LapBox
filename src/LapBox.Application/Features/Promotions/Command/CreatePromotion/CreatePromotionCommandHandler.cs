using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Promotions;
using MediatR;

namespace LapBox.Application.Features.Promotions.Command.CreatePromotion;

public sealed class CreatePromotionCommandHandler(IPromotionRepository promotionRepository) 
    : IRequestHandler<CreatePromotionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePromotionCommand command, CancellationToken ct)
    {
        // 1️⃣ استخدام الـ Domain Logic اللي أنت كاتبه بنفسك عشان نخلق الكوبون ونضمن سلامته
        var promotionResult = Promotion.Create(command.Code, command.DiscountPercentage, command.StartDate, command.EndDate);
        
        if (promotionResult.IsError)
        {
            return promotionResult.Errors;
        }

        // 2️⃣ الحفظ في قاعدة البيانات
        await promotionRepository.AddAsync(promotionResult.Value, ct);

        return promotionResult.Value.Id;
    }
}