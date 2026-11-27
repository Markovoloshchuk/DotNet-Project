using System.ComponentModel.DataAnnotations;

namespace Catalog.Bll.DTOs.MedicalServices;

public sealed class CreateMedicalServiceDto
{
    [Range(1, long.MaxValue,
        ErrorMessage = "Category ID must be greater than zero.")]
    public long CategoryId { get; set; }

    [Range(1, long.MaxValue,
        ErrorMessage = "Equipment ID must be greater than zero.")]
    public long? EquipmentId { get; set; }

    [Required(ErrorMessage = "Service name is required.")]
    [StringLength(
        150,
        MinimumLength = 1,
        ErrorMessage = "Service name must contain between 1 and 150 characters.")]
    public string ServiceName { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "0",
        "99999999.99",
        ErrorMessage = "Price must be between 0 and 99999999.99.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Duration must be greater than zero.")]
    public int DurationMinutes { get; set; }
}