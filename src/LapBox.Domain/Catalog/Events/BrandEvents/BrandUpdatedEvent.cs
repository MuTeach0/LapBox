using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.BrandEvents;

public record BrandUpdatedEvent(Guid BrandId) : DomainEvent;
