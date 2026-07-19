using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Promotions;

public static class PromotionErrors
{
    public static readonly Error CodeRequired =
        Error.Validation("Promotion.Code", "Promotion code cannot be empty.");

    public static readonly Error DiscountInvalid =
        Error.Validation("Promotion.Discount", "Discount percentage must be between 1 and 100.");

    public static readonly Error DatesInvalid =
        Error.Validation("Promotion.Dates", "End date must be after start date.");

    public static readonly Error AlreadyDeactivated =
        Error.Conflict("Promotion.Status", "Promotion is already deactivated.");
}