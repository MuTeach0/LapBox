namespace LapBox.Contracts.Orders;

public sealed record OrderItemResponse(
    Guid LaptopId,
    string LaptopName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Status,
    IReadOnlyList<OrderItemResponse> Items,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string? PromotionCode,
    decimal DiscountAmount,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? UpdatedOnUtc);

/// <summary>
/// Request for PlaceOrder - creates temporary reservations from cart.
/// CustomerId comes from authentication context.
/// </summary>
public sealed record PlaceOrderRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);

/// <summary>
/// Response from PlaceOrder - contains temporary reservation details.
/// CustomerId is actually the IdentityId/UserId since no Customer record exists yet.
/// </summary>
public sealed record PlaceOrderResponse(
    Guid OrderId,
    Guid IdentityId,
    List<ReservationItemResponse> Items,
    DateTimeOffset ExpiresAtUtc,
    string Status);

public sealed record ReservationItemResponse(
    Guid ReservationId,
    Guid LaptopId,
    int Quantity,
    decimal UnitPrice,
    string NameWithSpecs);

/// <summary>
/// Request for CreateOrder - finalizes order from temporary reservations.
/// </summary>
public sealed record CreateOrderRequest(
    string ShippingStreet,
    string ShippingCity,
    string ShippingCountry,
    string ShippingState,
    string ShippingZipCode);

/// <summary>
/// Response from CreateOrder - contains created order details.
/// </summary>
public sealed record CreateOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset OrderDate,
    decimal TotalAmount);

public sealed record UpdateOrderStatusRequest(string Status);

public sealed record GetOrdersRequest(
    Guid? CustomerId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10);
