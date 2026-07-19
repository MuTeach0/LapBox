namespace LapBox.Domain.Laptops.Events;

using LapBox.Domain.Common;

public record LaptopPriceChangedEvent(Guid LaptopId, decimal OldPrice, decimal NewPrice) : DomainEvent;