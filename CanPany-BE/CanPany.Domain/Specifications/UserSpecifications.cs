using CanPany.Domain.Entities;

namespace CanPany.Domain.Specifications;

/// <summary>
/// User specifications for domain logic
/// </summary>
public static class UserSpecifications
{
    public static ISpecification<User> IsActive()
    {
        return new ActiveUserSpecification();
    }

    public static ISpecification<User> HasRole(string role)
    {
        return new HasRoleSpecification(role);
    }

    public static ISpecification<User> IsNotLocked()
    {
        return new NotLockedUserSpecification();
    }
}

internal class ActiveUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        return !entity.IsLocked && (entity.LockedUntil == null || entity.LockedUntil > DateTime.UtcNow);
    }
}

internal class HasRoleSpecification : ISpecification<User>
{
    private readonly string _role;

    public HasRoleSpecification(string role)
    {
        _role = role;
    }

    public bool IsSatisfiedBy(User entity)
    {
        return entity.Role == _role;
    }
}

internal class NotLockedUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        return !entity.IsLocked && (entity.LockedUntil == null || entity.LockedUntil <= DateTime.UtcNow);
    }
}


