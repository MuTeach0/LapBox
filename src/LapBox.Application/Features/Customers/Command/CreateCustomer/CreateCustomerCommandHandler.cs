using Microsoft.Extensions.Caching.Hybrid;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using LapBox.Domain.Customers;

namespace LapBox.Application.Features.Customers.Command.CreateCustomer;

public sealed class CreateCustomerCommandHandler(ICustomerRepository customerRepository,
                                                IUnitOfWork unitOfWork,
                                                ILogger<CreateCustomerCommandHandler> logger,
                                                HybridCache cache)
: IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        logger.LogInformation("Starting creation process for customer with Email: {Email}", command.Email);

        var email = command.Email.Trim().ToLower();

        var exists = await customerRepository.ExistsByEmailAsync(email, ct);

        if (exists)
        {
            logger.LogWarning("Customer creation aborted. Email already exists.");

            return CustomerErrors.CustomerExists;
        }

        // 1. استدعاء الـ Factory Method من الـ Domain Entity
        var customerResult = Customer.Create(
            command.IdentityId,
            command.Name,
            email,
            command.PhoneNumber);

        // 2. التحقق من الفشل بناءً على خصائص الـ Result عندك (!IsSuccess)
        if (customerResult.IsError)
        {
            logger.LogWarning("Customer creation failed due to domain validation: [{Code}] {Description}",
                customerResult.TopError.Code,
                customerResult.TopError.Description);

            return customerResult.Errors;
        }

        // 3. إضافة العميل الجديد إلى الـ Repository
        await customerRepository.AddAsync(customerResult.Value, ct);

        // 4. تأكيد وحفظ التغييرات نهائياً في قاعدة البيانات (EF Core Commit)
        await unitOfWork.SaveChangesAsync(ct);

        // 5. تنظيف الكاش عبر الـ Interface الذي قمنا ببنائه سابقاً
        await cache.RemoveByTagAsync("customers", ct);

        logger.LogInformation("Customer created successfully with Id: {CustomerId}", customerResult.Value.Id);

        return customerResult.Value.Id;
    }
}