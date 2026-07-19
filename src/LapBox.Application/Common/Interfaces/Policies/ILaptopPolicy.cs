namespace LapBox.Application.Common.Interfaces.Policies;

public interface ILaptopPolicy
{
    Task<bool> IsStockAvailableAsync(Guid laptopId, int quantity, CancellationToken ct = default);
}