
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class MedicalServicePackageDto
{
    public long PackageId { get; set; }

    public string PackageName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? CustomNotes { get; set; }
}
