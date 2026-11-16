using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class MedicalService
{
    public long ServiceId { get; private set; }

    public long CategoryId { get; private set; }

    public long? EquipmentId { get; private set; }

    public string ServiceName { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public int DurationMinutes { get; private set; }

    public bool IsActive { get; private set; }

    public ServiceCategory Category { get; private set; } = null!;

    public Equipment? Equipment { get; private set; }

    public ServiceDetail? Detail { get; private set; }

    public ICollection<PackageService> PackageServices { get; private set; }
        = new List<PackageService>();

    private MedicalService()
    {
        // For EF Core
    }

    public MedicalService(
        long categoryId,
        string serviceName,
        decimal price,
        int durationMinutes,
        long? equipmentId = null)
    {
        ChangeCategory(categoryId);
        Rename(serviceName);
        ChangePrice(price);
        ChangeDuration(durationMinutes);
        AssignEquipment(equipmentId);

        IsActive = true;
    }

    public void ChangeCategory(long categoryId)
    {
        if (categoryId <= 0)
            throw new DomainException("Category ID must be positive.");

        CategoryId = categoryId;
    }

    public void Rename(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new DomainException("Service name is required.");

        if (serviceName.Length > 150)
            throw new DomainException(
                "Service name cannot exceed 150 characters.");

        ServiceName = serviceName.Trim();
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
            throw new DomainException(
                "Service price cannot be negative.");

        Price = price;
    }

    public void ChangeDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new DomainException(
                "Service duration must be greater than zero.");

        DurationMinutes = durationMinutes;
    }

    public void AssignEquipment(long? equipmentId)
    {
        if (equipmentId.HasValue && equipmentId.Value <= 0)
            throw new DomainException("Equipment ID must be positive.");

        EquipmentId = equipmentId;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}