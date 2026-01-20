using CanPany.Domain.Entities;

namespace CanPany.Domain.Specifications;

/// <summary>
/// Payment specifications for domain logic
/// </summary>
public static class PaymentSpecifications
{
    public static ISpecification<Payment> IsPending()
    {
        return new PendingPaymentSpecification();
    }

    public static ISpecification<Payment> IsCompleted()
    {
        return new CompletedPaymentSpecification();
    }

    public static ISpecification<Payment> IsFailed()
    {
        return new FailedPaymentSpecification();
    }

    public static ISpecification<Payment> HasMinimumAmount(long minAmount)
    {
        return new HasMinimumAmountSpecification(minAmount);
    }
}

internal class PendingPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Pending";
    }
}

internal class CompletedPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Paid";
    }
}

internal class FailedPaymentSpecification : ISpecification<Payment>
{
    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Status == "Failed";
    }
}

internal class HasMinimumAmountSpecification : ISpecification<Payment>
{
    private readonly long _minAmount;

    public HasMinimumAmountSpecification(long minAmount)
    {
        _minAmount = minAmount;
    }

    public bool IsSatisfiedBy(Payment entity)
    {
        return entity.Amount >= _minAmount;
    }
}


