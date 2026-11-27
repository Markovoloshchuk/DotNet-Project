using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain;

public sealed class ServiceCategoryTests
{
    [Fact]
    public void Constructor_ValidData_CreatesCategory()
    {
        var category = new ServiceCategory(
            "  Diagnostics  ",
            "Diagnostic medical services.");

        Assert.Equal("Diagnostics", category.CategoryName);
        Assert.Equal(
            "Diagnostic medical services.",
            category.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsDomainException(
        string categoryName)
    {
        Assert.Throws<DomainException>(() =>
            new ServiceCategory(categoryName));
    }

    [Fact]
    public void Constructor_NameLongerThan100Characters_Throws()
    {
        var categoryName = new string('A', 101);

        Assert.Throws<DomainException>(() =>
            new ServiceCategory(categoryName));
    }

    [Fact]
    public void UpdateDescription_DescriptionLongerThan500Characters_Throws()
    {
        var category = new ServiceCategory("Diagnostics");
        var description = new string('A', 501);

        Assert.Throws<DomainException>(() =>
            category.UpdateDescription(description));
    }

    [Fact]
    public void UpdateDescription_NullValue_IsAllowed()
    {
        var category = new ServiceCategory(
            "Diagnostics",
            "Some description");

        category.UpdateDescription(null);

        Assert.Null(category.Description);
    }

    [Fact]
    public void Rename_ValidName_UpdatesCategoryName()
    {
        var category = new ServiceCategory("Diagnostics");

        category.Rename("Laboratory Tests");

        Assert.Equal("Laboratory Tests", category.CategoryName);
    }
}