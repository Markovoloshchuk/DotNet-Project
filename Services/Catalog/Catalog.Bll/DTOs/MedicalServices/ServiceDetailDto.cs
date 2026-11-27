
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class ServiceDetailDto
{
    public long ServiceDetailId { get; set; }

    public string? PreparationInstructions { get; set; }

    public string? Contraindications { get; set; }
}
