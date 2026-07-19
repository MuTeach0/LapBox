using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Interfaces.Policies;
using LapBox.Application.Common.Interfaces.Services;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Orders.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Command.RemoveOrder;

public class RemoveOrderCommandHandler(
    ILogger<RemoveOrderCommandHandler> logger,
    IOrderRepository orderRepository,
    IOrderPolicy orderPolicy,
    // IDateTimeProvider provider,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<RemoveOrderCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(RemoveOrderCommand command, CancellationToken ct)
    {
        logger.LogInformation("User {UserId} with role {Role} attempting to cancel order {OrderId}", 
            command.CurrentUserId, command.CurrentUserRole, command.OrderId);

        // logger.LogInformation("Attempting to cancel/remove order with ID: {OrderId}", command.OrderId);

        var order = await orderRepository.GetOrderWithItemsAsync(command.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order with ID: {OrderId} was not found.", command.OrderId);

            // التعديل هنا: استدعاء الخطأ الجاهز من كلاسك الموحد
            return ApplicationErrors.OrderNotFound;
        }

        if (order.UserId != command.CurrentUserId && command.CurrentUserRole != "Admin")
        {
            logger.LogWarning("Unauthorized cancellation attempt! User {UserId} tried to cancel Order {OrderId} belonging to User {OwnerId}", 
                command.CurrentUserId, command.OrderId, order.UserId);

            return ApplicationErrors.OrderUnauthorized;
        }

        // 3. 🛡️ تطبيق الـ Order Policy باستخدام الـ DateTimeProvider
        // الـ Admin غالباً بيتخطى شروط وقت الإلغاء، فالسياسة بنطبقها بشكل أساسي على العميل
        if (command.CurrentUserRole != "Admin")
        {
            bool isDispatched = order.Status == OrderStatus.Dispatched;
            var canCancel = orderPolicy.CanCancelOrder(order.OrderDate, isDispatched); // 🧠 فحص شروط البزنس
            if (!canCancel)
            {
                logger.LogWarning("Order cancellation denied by Policy for Order ID {OrderId}. Time exceeded or already dispatched.", command.OrderId);
                return ApplicationErrors.OrderCannotCancel;
            }
        }

        Result<Success> cancelResult;
        if (command.CurrentUserRole == "Admin")
            cancelResult = order.UpdateStatus(OrderStatus.Cancelled);
        else
            cancelResult = order.CancelByCustomer();

        if (cancelResult.IsError)
        {
            logger.LogWarning("Order cancellation failed for ID {OrderId}. Reason: {Error}",
                command.OrderId, cancelResult.TopError.Description);
            return cancelResult.Errors;
        }
        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync("orders", ct);

        logger.LogInformation("Order with ID: {OrderId} successfully cancelled.", command.OrderId);

        return Result.Deleted;
    }
}