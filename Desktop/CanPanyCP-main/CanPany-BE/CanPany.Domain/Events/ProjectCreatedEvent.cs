using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

/// <summary>
/// Domain event raised when a project is created
/// </summary>
public class ProjectCreatedEvent : BaseDomainEvent
{
    public Project Project { get; }
    public string OwnerId { get; }

    public ProjectCreatedEvent(Project project, string ownerId)
    {
        Project = project;
        OwnerId = ownerId;
    }
}


