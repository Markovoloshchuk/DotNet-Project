
using AutoMapper;
using Catalog.Bll.Interfaces;
using Catalog.Bll.Mapping;
using Catalog.Bll.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Bll;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogBll(
        this IServiceCollection services,
        string? autoMapperLicenseKey = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Business services
        services.AddScoped<
            IServiceCategoryService,
            ServiceCategoryService>();

        services.AddScoped<
            IMedicalServiceService,
            MedicalServiceService>();

        // AutoMapper
        services.AddAutoMapper(
            cfg =>
            {
                if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
                {
                    cfg.LicenseKey = autoMapperLicenseKey;
                }
            },
            typeof(CatalogMappingProfile));

        return services;
    }
}
