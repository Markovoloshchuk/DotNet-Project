
namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class SearchMedicalServicesDto
{
    // Filters
    public string? Name { get; set; }

    public long? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    // Pagination
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    // Sorting
    public string SortBy { get; set; } = "serviceName";

    public bool SortDescending { get; set; }
}
