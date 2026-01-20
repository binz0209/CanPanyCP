namespace CanPany.Shared.Common.Base;

/// <summary>
/// Base class for all entities in the system
/// </summary>
public abstract class EntityBase
{
    public string Id { get; protected set; } = string.Empty;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    
    protected EntityBase()
    {
        Id = Guid.NewGuid().ToString();
    }
    
    protected EntityBase(string id)
    {
        Id = id;
    }
    
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

