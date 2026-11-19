using Catalog.Domain.Entities;

namespace Catalog.Domain.Specifications.MedicalServices;

public sealed class MedicalServicesByCategorySpecification
    : BaseSpecification<MedicalService>
{
    public MedicalServicesByCategorySpecification(
        long categoryId,
        bool activeOnly = false)
        : base(service =>
            service.CategoryId == categoryId &&
            (!activeOnly || service.IsActive))
    {
        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId),
                "Category ID must be greater than zero.");
        }

        AddInclude(service => service.Category);

        ApplyOrderBy(service => service.ServiceName);
    }
}