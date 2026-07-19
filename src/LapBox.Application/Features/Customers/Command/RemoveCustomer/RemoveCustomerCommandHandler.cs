using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Command.RemoveCustomer;

public class RemoveCustomerCommandHandler(
    ILogger<RemoveCustomerCommandHandler> logger,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache): IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(RemoveCustomerCommand command, CancellationToken ct)
    {
       logger.LogInformation("Attempting to remove customer with ID: {CustomerId}", command.CustomerId);

        // 1. جلب العميل للتأكد من وجوده
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
        {
            logger.LogWarning("Customer with ID: {CustomerId} was not found.", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        // 2. 🛡️ حماية البزنس: التحقق من وجود أي طلبات مرتبطة بالعميل
        var customerOrders = await orderRepository.GetByUserIdAsync(command.CustomerId, ct);
        if (customerOrders != null && customerOrders.Count > 0)
        {
            logger.LogWarning("Cannot delete customer {CustomerId} because they have {Count} existing orders.", 
                command.CustomerId, customerOrders.Count);
            
            return ApplicationErrors.CustomerHasOrders; // 👈 نمنع الحذف فوراً ونرجع خطأ كconflict
        }

        // 3. إذا عبر الشرط.. نقوم بالحذف السليم
        customerRepository.Remove(customer);
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ تنظيف الكاش
        await cache.RemoveByTagAsync("customers", ct);
        
        logger.LogInformation("Customer with ID: {CustomerId} was successfully removed.", command.CustomerId);

        return Result.Deleted;
    }
}