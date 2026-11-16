
namespace Catalog.Bll.DTOs.Categories;

public sealed class CreateServiceCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
