namespace LapBox.Domain.Common;

public abstract class AggregateRoot : AuditableEntity
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }
}