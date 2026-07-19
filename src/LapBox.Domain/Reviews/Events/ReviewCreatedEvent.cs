using LapBox.Domain.Common;

namespace LapBox.Domain.Reviews.Events;

public record ReviewCreatedEvent(
    Guid ReviewId,
    Guid LaptopId,
    Guid UserId,
    int Rating) : DomainEvent;