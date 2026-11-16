
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class SearchMedicalServicesDto
{
    public string? Name { get; set; }

    public long? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }
}
