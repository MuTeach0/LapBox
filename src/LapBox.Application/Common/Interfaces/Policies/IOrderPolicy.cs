namespace LapBox.Application.Common.Interfaces.Policies;

public interface IOrderPolicy
{
    bool CanCancelOrder(DateTimeOffset orderDate, bool isDispatched);
    bool CanReturnOrder(DateTimeOffset deliveredDate);
}