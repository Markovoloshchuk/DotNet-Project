
namespace Catalog.Bll.DTOs.Categories;

public sealed class UpdateServiceCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
