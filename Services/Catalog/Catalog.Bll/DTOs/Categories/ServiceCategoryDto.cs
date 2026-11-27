
namespace Catalog.Bll.DTOs.Categories;

public sealed class ServiceCategoryDto
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
