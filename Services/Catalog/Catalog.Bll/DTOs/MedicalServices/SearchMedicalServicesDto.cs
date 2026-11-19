using System.ComponentModel.DataAnnotations;

namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class SearchMedicalServicesDto
{
    // Filters

    [StringLength(150,
        ErrorMessage = "Search name cannot exceed 150 characters.")]
    public string? Name { get; set; }

    [Range(1, long.MaxValue,
        ErrorMessage = "Category ID must be greater than zero.")]
    public long? CategoryId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99",
        ErrorMessage = "Minimum price must be between 0 and 99999999.99.")]
    public decimal? MinPrice { get; set; }

    [Range(typeof(decimal), "0", "99999999.99",
        ErrorMessage = "Maximum price must be between 0 and 99999999.99.")]
    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    // Pagination

    [Range(1, int.MaxValue,
        ErrorMessage = "Page number must be greater than zero.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100,
        ErrorMessage = "Page size must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    // Sorting

    [StringLength(50,
        ErrorMessage = "Sort field cannot exceed 50 characters.")]
    public string SortBy { get; set; } = "serviceName";

    public bool SortDescending { get; set; }
}