using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class Equipment
{
    public long EquipmentId { get; private set; }

    public string EquipmentName { get; private set; } = string.Empty;

    public string SerialNumber { get; private set; } = string.Empty;

    public DateOnly PurchaseDate { get; private set; }

    public bool IsOperational { get; private set; }

    public ICollection<MedicalService> MedicalServices { get; private set; }
        = new List<MedicalService>();

    private Equipment()
    {
        // For EF Core
    }

    public Equipment(
        string equipmentName,
        string serialNumber,
        DateOnly purchaseDate)
    {
        Rename(equipmentName);
        ChangeSerialNumber(serialNumber);

        PurchaseDate = purchaseDate;
        IsOperational = true;
    }

    public void Rename(string equipmentName)
    {
        if (string.IsNullOrWhiteSpace(equipmentName))
            throw new DomainException("Equipment name is required.");

        if (equipmentName.Length > 150)
            throw new DomainException(
                "Equipment name cannot exceed 150 characters.");

        EquipmentName = equipmentName.Trim();
    }

    public void ChangeSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new DomainException("Serial number is required.");

        if (serialNumber.Length > 50)
            throw new DomainException(
                "Serial number cannot exceed 50 characters.");

        SerialNumber = serialNumber.Trim();
    }

    public void MarkOperational()
    {
        IsOperational = true;
    }

    public void MarkOutOfService()
    {
        IsOperational = false;
    }
}