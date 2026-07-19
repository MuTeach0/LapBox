using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.BrandEvents;

public record BrandCreatedEvent(Guid BrandId) : DomainEvent;