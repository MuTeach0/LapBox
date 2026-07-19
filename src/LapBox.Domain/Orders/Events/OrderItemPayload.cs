namespace LapBox.Domain.Orders.Events;

// public record OrderItemPayload(string LaptopNameWithSpecs, int Quantity, decimal UnitPrice);
public record OrderItemPayload(Guid LaptopId, string LaptopNameWithSpecs, int Quantity, decimal UnitPrice);