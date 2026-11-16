
using AutoMapper;
using Catalog.Bll.DTOs.Categories;
using Catalog.Bll.Interfaces;
using Catalog.Dal.UnitOfWork;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Bll.Services;

public sealed class ServiceCategoryService : IServiceCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ServiceCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var categories = await _unitOfWork.ServiceCategories
            .GetAllAsync(ct);

        return _mapper.Map<IReadOnlyList<ServiceCategoryDto>>(
            categories);
    }

    public async Task<ServiceCategoryDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task<ServiceCategoryDto> CreateAsync(
        CreateServiceCategoryDto dto,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var category = new ServiceCategory(
            dto.CategoryName,
            dto.Description);

        await _unitOfWork.ServiceCategories.AddAsync(
            category, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task<ServiceCategoryDto> UpdateAsync(
        long id,
        UpdateServiceCategoryDto dto,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        category.Rename(dto.CategoryName);
        category.UpdateDescription(dto.Description);

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        _unitOfWork.ServiceCategories.Remove(category);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
