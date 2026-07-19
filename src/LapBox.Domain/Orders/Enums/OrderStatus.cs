namespace LapBox.Domain.Orders.Enums;

public enum OrderStatus
{
    Created,             // Order Created (من المخطط)
    Placed,             // Order Placed (من المخطط)
    PaymentProcessed,   // Payment Processed (من المخطط)
    InventoryReserved,  // Reserve Stock (من المخطط)
    Packaged,           // Package Order (من المخطط)
    Dispatched,         // Order Dispatched (من المخطط)
    Delivered,          // Confirm Delivery (من المخطط)
    Cancelled           // Cancel Order (من المخطط)
}