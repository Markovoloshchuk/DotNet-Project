
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class CreateMedicalServiceDto
{
    public long CategoryId { get; set; }

    public long? EquipmentId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationMinutes { get; set; }
}
