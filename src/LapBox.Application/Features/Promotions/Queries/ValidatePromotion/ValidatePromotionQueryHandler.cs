using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Promotions.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Promotions.Queries.ValidatePromotion;

public sealed class ValidatePromotionQueryHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<ValidatePromotionQuery, Result<PromotionDTO>>
{
    public async Task<Result<PromotionDTO>> Handle(ValidatePromotionQuery request, CancellationToken ct)
    {
        // 1️⃣ جلب الكوبون باستخدام الـ Method المخصصة بتاعتك في الـ Repository
        var promotion = await promotionRepository.GetByCodeAsync(request.Code.ToUpperInvariant(), ct);
        if (promotion is null)
        {
            return ApplicationErrors.PromotionNotFound; // 🎯 خطأك الموحد
        }

        // 2️⃣ التشيك على الصلاحية باستخدام دالة الـ Domain الذكية بتاعتك
        if (!promotion.IsEligible(DateTimeOffset.UtcNow))
        {
            return ApplicationErrors.PromotionNotEligible(request.Code); // 🎯 خطأك الموحد
        }

        // 3️⃣ إرجاع الـ DTO بنجاح (لاحظ استخدمنا DateTimeOffset متوافق مع الدومين)
        return new PromotionDTO(promotion.Id, promotion.Code, promotion.DiscountPercentage, promotion.EndDate);
    }
}