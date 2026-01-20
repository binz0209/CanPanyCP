using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

/// <summary>
/// Domain event raised when a notification is created
/// </summary>
public class NotificationCreatedEvent : BaseDomainEvent
{
    public Notification Notification { get; }

    public NotificationCreatedEvent(Notification notification)
    {
        Notification = notification;
    }
}


