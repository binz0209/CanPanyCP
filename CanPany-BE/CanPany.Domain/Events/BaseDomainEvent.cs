using CanPany.Shared.Common.Base;

namespace CanPany.Domain.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
    public string EventId { get; protected set; } = Guid.NewGuid().ToString();
}


