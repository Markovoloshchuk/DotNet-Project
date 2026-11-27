using Catalog.Dal.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Specifications;

internal static class TestCatalogDbContextFactory
{
    public static CatalogDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(
                $"CatalogTests_{Guid.NewGuid():N}")
            .Options;

        return new CatalogDbContext(options);
    }
}