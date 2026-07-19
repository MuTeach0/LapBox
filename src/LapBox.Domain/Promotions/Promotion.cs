using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Promotions.Events;

namespace LapBox.Domain.Promotions;

public sealed class Promotion : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public decimal DiscountPercentage { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private Promotion() { }

    private Promotion(Guid id, string code, decimal discountPercentage, DateTimeOffset startDate, DateTimeOffset endDate) : base(id)
    {
        Code = code.ToUpperInvariant();
        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    public static Result<Promotion> Create(string code, decimal discountPercentage, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (string.IsNullOrWhiteSpace(code))
            return PromotionErrors.CodeRequired;

        if (discountPercentage <= 0 || discountPercentage > 100)
            return PromotionErrors.DiscountInvalid;

        if (endDate <= startDate)
            return PromotionErrors.DatesInvalid;

        var promotion = new Promotion(Guid.NewGuid(), code, discountPercentage, startDate, endDate);
        promotion.AddDomainEvent(new PromotionCreatedEvent(promotion.Id, promotion.Code, promotion.DiscountPercentage));

        return promotion;
    }

    public Result<Success> Deactivate()
    {
        if (!IsActive)
            return PromotionErrors.AlreadyDeactivated;

        IsActive = false;
        return Result.Success;
    }

    // دالة مهمة للتحقق هل الكود صالح للاستخدام أم لا
    public bool IsEligible(DateTimeOffset currentDate)
       => IsActive && currentDate >= StartDate && currentDate <= EndDate;
}