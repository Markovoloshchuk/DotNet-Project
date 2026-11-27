using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class ServiceCategory
{
    public long CategoryId { get; private set; }

    public string CategoryName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ICollection<MedicalService> MedicalServices { get; private set; }
        = new List<MedicalService>();

    private ServiceCategory()
    {
        // For EF Core
    }

    public ServiceCategory(string categoryName, string? description = null)
    {
        Rename(categoryName);
        UpdateDescription(description);
    }

    public void Rename(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        if (categoryName.Length > 100)
            throw new DomainException(
                "Category name cannot exceed 100 characters.");

        CategoryName = categoryName.Trim();
    }

    public void UpdateDescription(string? description)
    {
        if (description?.Length > 500)
            throw new DomainException(
                "Category description cannot exceed 500 characters.");

        Description = description;
    }
}