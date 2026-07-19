using LapBox.Application.Common.Interfaces.Policies;

namespace LapBox.Infrastructure.Services;

public sealed class OrderPolicy : IOrderPolicy
{
    // يمكن إلغاء الطلب خلال 24 ساعة من الإنشاء، بشرط عدم شحنه بعد
    public bool CanCancelOrder(DateTimeOffset orderDate, bool isDispatched)
    {
        if (isDispatched) return false;

        var hoursSinceOrder = (DateTimeOffset.UtcNow - orderDate).TotalHours;
        return hoursSinceOrder <= 24;
    }

    // يمكن إرجاع الطلب خلال 14 يوم من تاريخ التسليم
    public bool CanReturnOrder(DateTimeOffset deliveredDate)
    {
        return (DateTimeOffset.UtcNow - deliveredDate).TotalDays <= 14;
    }
}
