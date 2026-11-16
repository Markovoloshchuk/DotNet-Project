using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class ServiceDetail
{
    public long ServiceDetailId { get; private set; }

    public long ServiceId { get; private set; }

    public string? PreparationInstructions { get; private set; }

    public string? Contraindications { get; private set; }

    public MedicalService Service { get; private set; } = null!;

    private ServiceDetail()
    {
        // For EF Core
    }

    public ServiceDetail(
        long serviceId,
        string? preparationInstructions = null,
        string? contraindications = null)
    {
        if (serviceId <= 0)
            throw new DomainException("Service ID must be positive.");

        ServiceId = serviceId;
        UpdatePreparationInstructions(preparationInstructions);
        UpdateContraindications(contraindications);
    }

    public void UpdatePreparationInstructions(string? instructions)
    {
        PreparationInstructions = instructions;
    }

    public void UpdateContraindications(string? contraindications)
    {
        Contraindications = contraindications;
    }
}