using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NotPlaced =
        Error.Conflict("Order.Status", "Cannot add items to an order that is not placed.");

    public static readonly Error ItemExists =
        Error.Conflict("Order.ItemExists", "This laptop is already in the order.");

    public static readonly Error QuantityInvalid =
        Error.Validation("Order.Quantity", "Quantity must be at least 1.");
    public static readonly Error InvalidStatusTransition =
        Error.Validation("Order.InvalidStatusTransition", "Cannot transition order status from '{0}' to '{1}'.");
    public static readonly Error ItemNotFound =
        Error.NotFound("Order.ItemNotFound", "Laptop not found in the order.");

    public static readonly Error StatusUnchangeable =
        Error.Conflict("Order.Status", "Cannot change status of a completed or cancelled order.");

    public static readonly Error NotPackaged =
        Error.Conflict("Order.Status", "Order must be packaged before it can be dispatched.");

    public static readonly Error TrackingRequired =
        Error.Validation("Order.Tracking", "Tracking label cannot be empty.");
}