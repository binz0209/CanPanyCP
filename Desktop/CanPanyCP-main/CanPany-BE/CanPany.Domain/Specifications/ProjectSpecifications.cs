using CanPany.Domain.Entities;

namespace CanPany.Domain.Specifications;

/// <summary>
/// Project specifications for domain logic
/// </summary>
public static class ProjectSpecifications
{
    public static ISpecification<Project> IsOpen()
    {
        return new OpenProjectSpecification();
    }

    public static ISpecification<Project> IsOwnedBy(string ownerId)
    {
        return new OwnedBySpecification(ownerId);
    }

    public static ISpecification<Project> HasBudget(decimal minBudget)
    {
        return new HasMinimumBudgetSpecification(minBudget);
    }

    public static ISpecification<Project> IsInCategory(string categoryId)
    {
        return new InCategorySpecification(categoryId);
    }
}

internal class OpenProjectSpecification : ISpecification<Project>
{
    public bool IsSatisfiedBy(Project entity)
    {
        return entity.Status == "Open";
    }
}

internal class OwnedBySpecification : ISpecification<Project>
{
    private readonly string _ownerId;

    public OwnedBySpecification(string ownerId)
    {
        _ownerId = ownerId;
    }

    public bool IsSatisfiedBy(Project entity)
    {
        return entity.OwnerId == _ownerId;
    }
}

internal class HasMinimumBudgetSpecification : ISpecification<Project>
{
    private readonly decimal _minBudget;

    public HasMinimumBudgetSpecification(decimal minBudget)
    {
        _minBudget = minBudget;
    }

    public bool IsSatisfiedBy(Project entity)
    {
        return entity.BudgetAmount.HasValue && entity.BudgetAmount.Value >= _minBudget;
    }
}

internal class InCategorySpecification : ISpecification<Project>
{
    private readonly string _categoryId;

    public InCategorySpecification(string categoryId)
    {
        _categoryId = categoryId;
    }

    public bool IsSatisfiedBy(Project entity)
    {
        return entity.CategoryId == _categoryId;
    }
}


