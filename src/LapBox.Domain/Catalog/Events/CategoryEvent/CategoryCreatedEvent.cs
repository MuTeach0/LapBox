using LapBox.Domain.Common;

namespace LapBox.Domain.Catalog.Events.CategoryEvents;

public record CategoryCreatedEvent(Guid CategoryId) : DomainEvent;