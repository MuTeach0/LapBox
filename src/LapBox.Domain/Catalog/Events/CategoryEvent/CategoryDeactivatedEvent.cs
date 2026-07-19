using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.CategoryEvents;

public record CategoryDeactivatedEvent(Guid CategoryId) : DomainEvent;
