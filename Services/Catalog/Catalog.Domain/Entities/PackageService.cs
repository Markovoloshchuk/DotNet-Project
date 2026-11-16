using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class PackageService
{
    public long PackageId { get; private set; }

    public long ServiceId { get; private set; }

    public int Quantity { get; private set; }

    public string? CustomNotes { get; private set; }

    public ServicePackage Package { get; private set; } = null!;

    public MedicalService Service { get; private set; } = null!;

    private PackageService()
    {
        // For EF Core
    }

    public PackageService(
        long packageId,
        long serviceId,
        int quantity = 1,
        string? customNotes = null)
    {
        if (packageId <= 0)
            throw new DomainException("Package ID must be positive.");

        if (serviceId <= 0)
            throw new DomainException("Service ID must be positive.");

        PackageId = packageId;
        ServiceId = serviceId;

        ChangeQuantity(quantity);
        UpdateCustomNotes(customNotes);
    }

    public void ChangeQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException(
                "Quantity must be greater than zero.");

        Quantity = quantity;
    }

    public void UpdateCustomNotes(string? customNotes)
    {
        if (customNotes?.Length > 200)
            throw new DomainException(
                "Custom notes cannot exceed 200 characters.");

        CustomNotes = customNotes;
    }
}