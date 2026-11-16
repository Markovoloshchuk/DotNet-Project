
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class MedicalServiceDto
{
    public long ServiceId { get; set; }

    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public long? EquipmentId { get; set; }

    public string? EquipmentName { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; }

    public ServiceDetailDto? Detail { get; set; }

    public IReadOnlyList<MedicalServicePackageDto> Packages { get; set; }
        = Array.Empty<MedicalServicePackageDto>();
}
