namespace Shopilent.Domain.Common.Specifications;

public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override bool IsSatisfiedBy(T entity)
    {
        return !_specification.IsSatisfiedBy(entity);
    }
}