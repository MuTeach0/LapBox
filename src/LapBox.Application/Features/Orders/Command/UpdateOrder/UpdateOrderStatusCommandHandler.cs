using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Orders.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Command.UpdateOrder;

public class UpdateOrderStatusCommandHandler(
    ILogger<UpdateOrderStatusCommandHandler> logger,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<UpdateOrderStatusCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateOrderStatusCommand command, CancellationToken ct)
    {
        logger.LogInformation("Admin/Staff is attempting to update status for Order {OrderId} to {NewStatus}", 
            command.OrderId, command.NewStatus);

        // 1. جلب الطلب بكامل تفاصيله
        var order = await orderRepository.GetOrderWithItemsAsync(command.OrderId, ct);
        if (order is null) 
        {
            logger.LogWarning("Order status update failed. Order {OrderId} not found.", command.OrderId);
            return ApplicationErrors.OrderNotFound;
        }

        // 2. تطبيق منطق الـ Switch الذكي الخاص بك
        var result = command.NewStatus switch
        {
            OrderStatus.Dispatched => order.DispatchOrder(command.TrackingLabel!),
            _ => order.UpdateStatus(command.NewStatus)
        };

        // 3. التحقق من قيود البزنس في الـ Domain (مثلاً: يمنع التحويل لـ Dispatched لو لم يكن Packaged)
        if (result.IsError) 
        {
            logger.LogWarning("Domain validation failed for updating Order {OrderId} to {NewStatus}. Reason: {Error}", 
                command.OrderId, command.NewStatus, result.TopError.Description);
            return result.Errors;
        }

        // 4. الحفظ في قاعدة البيانات
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 5. تطهير كاش الطلبات فوراً لكي يرى العميل والإدارة الحالة الجديدة للطلب في نفس اللحظة
        await cache.RemoveByTagAsync("orders", ct);

        logger.LogInformation("Order {OrderId} status successfully updated to {NewStatus}.", command.OrderId, command.NewStatus);

        return Result.Updated;
    }
}