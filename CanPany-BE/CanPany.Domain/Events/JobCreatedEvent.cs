using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

/// <summary>
/// Domain event raised when a new job is created
/// </summary>
public class JobCreatedEvent : BaseDomainEvent
{
    public Job Job { get; }
    public string CompanyId { get; }

    public JobCreatedEvent(Job job, string companyId)
    {
        Job = job;
        CompanyId = companyId;
    }
}

