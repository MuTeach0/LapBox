using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Contracts.Orders;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers;
using LapBox.Domain.Orders;
using LapBox.Domain.Orders.Events;
using LapBox.Domain.Orders.ValueObjects;
using LapBox.Domain.StockReservations;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Command.CreateOrder;

/// <summary>
/// Handles Order creation from temporary stock reservations.
///
/// FLOW: Register (no role) → PlaceOrder (reserve) → Payment → CreateOrder (finalize)
///
/// This command:
/// 1. Finds active reservations for the user (by IdentityId)
/// 2. Creates Customer record if user doesn't have one (first order ever)
/// 3. Adds "Customer" role (user has NO role when first registering)
/// 4. Creates Order with items from reservations
/// 5. Consumes reservations (link them to Order)
/// 6. Triggers OrderCreatedEvent
/// </summary>
public class CreateOrderCommandHandler(
    ILogger<CreateOrderCommandHandler> logger,
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    ILaptopRepository laptopRepository,
    IStockReservationRepository reservationRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        logger.LogInformation(
            "Starting order creation for Identity ID: {IdentityId}", 
            command.IdentityId);

        // 1️⃣ Check if Customer record exists, if not create one (first order ever = first Customer)
        // This also upgrades the user from no-role to "Customer" role
        var customer = await customerRepository.GetByIdentityIdAsync(command.IdentityId, ct);
        Guid customerId;

        if (customer is null)
        {
            logger.LogInformation(
                "User {IdentityId} is placing their first order. Creating Customer profile.",
                command.IdentityId);

            // Get user details from Identity to create Customer
            var userDetailsResult = await identityService.GetUserByIdAsync(command.IdentityId, ct);
            if (userDetailsResult.IsError) return userDetailsResult.Errors;
            var userDto = userDetailsResult.Value;

            string fullName = $"{userDto.FirstName} {userDto.LastName}".Trim();

            // Create Customer record
            var customerResult = Customer.Create(
                command.IdentityId,
                fullName,
                userDto.Email,
                null);

            if (customerResult.IsError) return customerResult.Errors;
            customer = customerResult.Value;

            await customerRepository.AddAsync(customer, ct);
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation(
                "Created Customer {CustomerId} for Identity {IdentityId}", 
                customer.Id, 
                command.IdentityId);
        }

        customerId = customer.Id;

        // 2️⃣ Add "Customer" role if user doesn't have it yet
        // (User has NO role when first registering, we upgrade to Customer here)
        if (!await identityService.IsInRoleAsync(command.IdentityId, "Customer"))
        {
            // Add "Customer" role directly (user has no role currently)
            await identityService.AddToRoleAsync(command.IdentityId, "Customer", ct);
            logger.LogInformation("Upgraded Identity {IdentityId} to Customer role", command.IdentityId);
        }

        // 3️⃣ جلب الـ Reservations النشطة للعميل (by IdentityId/UserId)
        var reservations = await reservationRepository.GetActiveByUserIdAsync(command.IdentityId, ct);

        if (reservations.Count == 0)
        {
            logger.LogWarning("No active reservations found for user {IdentityId}", command.IdentityId);
            return StockReservationErrors.NoActiveReservations;
        }

        // 4️⃣ فلترة الـ Reservations النشطة فقط (مش منتهية)
        var now = DateTimeOffset.UtcNow;
        var activeReservations = reservations
            .Where(r => r.Status == Domain.StockReservations.Enums.ReservationStatus.Active && r.ExpiresAtUtc > now)
            .ToList();

        if (activeReservations.Count == 0)
        {
            logger.LogWarning("All reservations for user {IdentityId} have expired", command.IdentityId);
            return StockReservationErrors.Expired;
        }

        // 5️⃣ جلب الـ Laptops للحصول على السعر والـ specs
        var laptopIds = activeReservations.Select(r => r.LaptopId).ToList();
        var laptops = await laptopRepository.GetByIdsAsync(laptopIds, ct);
        var laptopsDict = laptops.ToDictionary(l => l.Id);

        // 6️⃣ بناء Order Items من الـ Reservations
        var orderItemsPayload = new List<(Guid LaptopId, int Quantity, decimal UnitPrice, decimal Discount, OrderItemPayload orderItem)>();
       foreach (var reservation in activeReservations)
        {
            if (!laptopsDict.TryGetValue(reservation.LaptopId, out var laptop))
            {
                logger.LogError("Laptop {LaptopId} from reservation not found", reservation.LaptopId);
                return Error.NotFound("Laptop.NotFound", $"Laptop with ID {reservation.LaptopId} was not found.");
            }

            // تجهيز الاسم الذي يحتاجه الـ Payload الجديد
            string nameWithSpecs = $"{laptop.Name} ({laptop.Specification.Processor} / {laptop.Specification.RAM})";

            // بناء الـ Payload (مطابق للـ Record الجديد)
            var payload = new OrderItemPayload(laptop.Id, nameWithSpecs, reservation.Quantity, laptop.BasePrice);

            orderItemsPayload.Add((
                laptop.Id,
                reservation.Quantity,
                laptop.BasePrice,
                0, // Discount
                payload
            ));
        }

        // 7️⃣ Transaction: إنشاء Order + consume Reservations
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // بناء عنوان الشحن
            var shippingAddress = new ShippingAddress(
                command.ShippingStreet,
                command.ShippingCity,
                command.ShippingCountry,
                command.ShippingState,
                command.ShippingZipCode
            );

            // إنشاء الـ Order
            var orderResult = Order.Create(command.IdentityId, shippingAddress, [.. orderItemsPayload.Select(i => i.orderItem)]);
            if (orderResult.IsError)
            {
                logger.LogError("Failed to create order: {Errors}", string.Join(", ", orderResult.Errors.Select(e => e.Description)));
                return orderResult.Errors;
            }
            var order = orderResult.Value;

            // إضافة الـ Order للـ Repository
            await orderRepository.AddAsync(order, ct);

            // consume كل الـ Reservations وربطها بالـ Order
            foreach (var reservation in activeReservations)
            {
                var consumeResult = reservation.ConsumeForOrder(order.Id);
                if (consumeResult.IsError)
                {
                    logger.LogError(
                        "Failed to consume reservation {ReservationId}: {Error}",
                        reservation.Id,
                        consumeResult.Errors.First().Description);
                    return consumeResult.Errors;
                }
            }

            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Invalidate cache
            await cache.RemoveByTagAsync("orders", ct);

            logger.LogInformation(
                "Order {OrderId} created successfully for Customer {CustomerId} (Identity: {IdentityId}) from {ReservationCount} reservations",
                order.Id,
                customerId,
                command.IdentityId,
                activeReservations.Count);

            return new CreateOrderResponse(
                order.Id,
                customerId,
                order.OrderDate,
                order.CalculateTotal()
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Failed to create order for Identity {IdentityId}", command.IdentityId);
            throw;
        }
    }
}
