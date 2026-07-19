using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.CategoryEvents;

public record CategoryUpdatedEvent(Guid CategoryId) : DomainEvent;
