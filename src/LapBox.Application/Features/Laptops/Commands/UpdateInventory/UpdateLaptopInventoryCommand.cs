using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.UpdateInventory;

// 📝 الـ Request يستقبل معرف اللابتوب والكمية المراد تعديلها (بالزيادة أو النقصان)
public record UpdateLaptopInventoryCommand(Guid LaptopId, int QuantityChange) : IRequest<Result<Success>>;