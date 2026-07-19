using LapBox.Domain.Common;

namespace LapBox.Domain.Laptops.Events;

public record LaptopDeactivatedEvent(Guid LaptopId) : DomainEvent;