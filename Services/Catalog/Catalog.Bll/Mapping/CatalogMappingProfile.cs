
using AutoMapper;
using Catalog.Bll.DTOs.Categories;
using Catalog.Bll.DTOs.MedicalServices;
using Catalog.Domain.Entities;

namespace Catalog.Bll.Mapping;

public sealed class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // Service Category
        CreateMap<ServiceCategory, ServiceCategoryDto>();

        // Service Details
        CreateMap<ServiceDetail, ServiceDetailDto>();

        // Package information associated with a medical service
        CreateMap<PackageService, MedicalServicePackageDto>()
            .ForMember(
                destination => destination.PackageName,
                options => options.MapFrom(
                    source => source.Package.PackageName));

        // Medical Service
        CreateMap<MedicalService, MedicalServiceDto>()
            .ForMember(
                destination => destination.CategoryName,
                options => options.MapFrom(
                    source => source.Category.CategoryName))
            .ForMember(
                destination => destination.EquipmentName,
                options => options.MapFrom(
                    source => source.Equipment == null
                        ? null
                        : source.Equipment.EquipmentName))
            .ForMember(
                destination => destination.Detail,
                options => options.MapFrom(
                    source => source.Detail))
            .ForMember(
                destination => destination.Packages,
                options => options.MapFrom(
                    source => source.PackageServices));
    }
}
