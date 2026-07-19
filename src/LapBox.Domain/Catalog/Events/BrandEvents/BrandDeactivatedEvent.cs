using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.BrandEvents;

public record BrandDeactivatedEvent(Guid BrandId) : DomainEvent;