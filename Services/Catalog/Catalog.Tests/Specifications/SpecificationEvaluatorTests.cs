using Catalog.Dal.Repositories;
using Catalog.Domain.Entities;
using Catalog.Domain.Specifications;
using Catalog.Domain.Specifications.MedicalServices;

namespace Catalog.Tests.Specifications;

public sealed class SpecificationEvaluatorTests
{
    [Fact]
    public async Task ActiveSpecification_ReturnsOnlyActiveServicesOrderedByName()
    {
        await using var context = TestCatalogDbContextFactory.Create();

        await SeedCatalogAsync(context);

        var repository = new GenericRepository<MedicalService>(context);

        var result = await repository.ListAsync(
            new ActiveMedicalServicesSpecification());

        Assert.Equal(3, result.Count);

        Assert.Equal(
            new[]
            {
                "Blood Count",
                "Electrocardiography",
                "Ultrasound"
            },
            result.Select(service => service.ServiceName));

        Assert.All(
            result,
            service => Assert.True(service.IsActive));

        // The specification includes the Category navigation property.
        Assert.All(
            result,
            service => Assert.NotNull(service.Category));

        Assert.Equal(
            "Diagnostics",
            result[1].Category.CategoryName);
    }

    [Fact]
    public async Task CategorySpecification_FiltersByCategoryAndActiveStatus()
    {
        await using var context = TestCatalogDbContextFactory.Create();

        var (diagnostics, _) = await SeedCatalogAsync(context);

        var repository = new GenericRepository<MedicalService>(context);

        var activeOnly = await repository.ListAsync(
            new MedicalServicesByCategorySpecification(
                diagnostics.CategoryId,
                activeOnly: true));

        var allInCategory = await repository.ListAsync(
            new MedicalServicesByCategorySpecification(
                diagnostics.CategoryId));

        Assert.Equal(2, activeOnly.Count);
        Assert.All(
            activeOnly,
            service => Assert.True(service.IsActive));

        Assert.Equal(3, allInCategory.Count);
        Assert.All(
            allInCategory,
            service => Assert.Equal(
                diagnostics.CategoryId,
                service.CategoryId));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void CategorySpecification_InvalidCategoryId_Throws(
        long categoryId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MedicalServicesByCategorySpecification(categoryId));
    }

    [Fact]
    public async Task SpecificationWithPaging_ReturnsRequestedPage()
    {
        await using var context = TestCatalogDbContextFactory.Create();

        await SeedCatalogAsync(context);

        var repository = new GenericRepository<MedicalService>(context);

        var result = await repository.ListAsync(
            new PricePagingSpecification(
                skip: 1,
                take: 2));

        Assert.Equal(2, result.Count);

        Assert.Equal(
            new[] { 350m, 450m },
            result.Select(service => service.Price));
    }

    private static async Task<(
        ServiceCategory Diagnostics,
        ServiceCategory Laboratory)> SeedCatalogAsync(
        Catalog.Dal.Persistence.CatalogDbContext context)
    {
        var diagnostics = new ServiceCategory(
            "Diagnostics",
            "Diagnostic medical services.");

        var laboratory = new ServiceCategory(
            "Laboratory Tests",
            "Laboratory medical services.");

        context.ServiceCategories.AddRange(
            diagnostics,
            laboratory);

        await context.SaveChangesAsync();

        var ultrasound = new MedicalService(
            diagnostics.CategoryId,
            "Ultrasound",
            850m,
            20);

        var electrocardiography = new MedicalService(
            diagnostics.CategoryId,
            "Electrocardiography",
            450m,
            15);

        var retiredService = new MedicalService(
            diagnostics.CategoryId,
            "Retired X-Ray",
            200m,
            10);

        retiredService.Deactivate();

        var bloodCount = new MedicalService(
            laboratory.CategoryId,
            "Blood Count",
            350m,
            15);

        context.MedicalServices.AddRange(
            ultrasound,
            electrocardiography,
            retiredService,
            bloodCount);

        await context.SaveChangesAsync();

        return (diagnostics, laboratory);
    }

    private sealed class PricePagingSpecification
        : BaseSpecification<MedicalService>
    {
        public PricePagingSpecification(int skip, int take)
        {
            ApplyOrderBy(service => service.Price);
            ApplyPaging(skip, take);
        }
    }
}