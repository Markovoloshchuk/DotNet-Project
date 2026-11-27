using Catalog.Domain.Entities;

namespace Catalog.Domain.Specifications.MedicalServices;

public sealed class ActiveMedicalServicesSpecification
    : BaseSpecification<MedicalService>
{
    public ActiveMedicalServicesSpecification()
        : base(service => service.IsActive)
    {
        AddInclude(service => service.Category);

        ApplyOrderBy(service => service.ServiceName);
    }
}