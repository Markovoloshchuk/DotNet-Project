using System.ComponentModel.DataAnnotations;

namespace Catalog.Bll.DTOs.Categories;

public sealed class CreateServiceCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(
        100,
        MinimumLength = 1,
        ErrorMessage = "Category name must contain between 1 and 100 characters.")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }
}