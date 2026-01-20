namespace CanPany.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityType, string entityId)
        : base($"Entity of type '{entityType}' with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}


