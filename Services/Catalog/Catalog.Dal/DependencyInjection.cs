
using Catalog.Dal.Interfaces;
using Catalog.Dal.Repositories;
using Catalog.Dal.UnitOfWork;
using CatalogUnitOfWork = Catalog.Dal.UnitOfWork.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Dal;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogDal(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Generic repository
        services.AddScoped(
            typeof(IGenericRepository<>),
            typeof(GenericRepository<>));

        // Specialized medical service repository
        services.AddScoped<
            IMedicalServiceRepository,
            MedicalServiceRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, CatalogUnitOfWork>();

        return services;
    }
}
