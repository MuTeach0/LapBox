namespace LapBox.Application.Common.Interfaces;

public interface IOrderCheckService
{
    /// <summary>
    /// يتحقق مما إذا كان المستخدم قد اشترى لابتوب معين بالفعل وحالة الطلب تم تسليمها
    /// </summary>
    Task<bool> HasUserPurchasedLaptopAsync(Guid userId, Guid laptopId, CancellationToken cancellationToken = default);
}