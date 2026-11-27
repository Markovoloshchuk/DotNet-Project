using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities;

public sealed class ServicePackage
{
    public long PackageId { get; private set; }

    public string PackageName { get; private set; } = string.Empty;

    public decimal TotalPrice { get; private set; }

    public decimal DiscountPercentage { get; private set; }

    public ICollection<PackageService> PackageServices { get; private set; }
        = new List<PackageService>();

    private ServicePackage()
    {
        // For EF Core
    }

    public ServicePackage(
        string packageName,
        decimal totalPrice,
        decimal discountPercentage = 0)
    {
        Rename(packageName);
        ChangeTotalPrice(totalPrice);
        ChangeDiscount(discountPercentage);
    }

    public void Rename(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName))
            throw new DomainException("Package name is required.");

        if (packageName.Length > 150)
            throw new DomainException(
                "Package name cannot exceed 150 characters.");

        PackageName = packageName.Trim();
    }

    public void ChangeTotalPrice(decimal totalPrice)
    {
        if (totalPrice < 0)
            throw new DomainException(
                "Package total price cannot be negative.");

        TotalPrice = totalPrice;
    }

    public void ChangeDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new DomainException(
                "Discount must be between 0 and 100 percent.");

        DiscountPercentage = discountPercentage;
    }
}