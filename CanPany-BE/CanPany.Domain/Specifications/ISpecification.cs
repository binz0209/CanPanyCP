namespace CanPany.Domain.Specifications;

/// <summary>
/// Specification pattern interface for domain logic
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}


