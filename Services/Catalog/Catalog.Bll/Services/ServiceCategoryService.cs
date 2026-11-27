using System.Diagnostics;
using AutoMapper;
using Catalog.Bll.DTOs.Categories;
using Catalog.Bll.Interfaces;
using Catalog.Dal.UnitOfWork;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Catalog.Bll.Services;

public sealed class ServiceCategoryService : IServiceCategoryService
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Catalog");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceCategoryService> _logger;

    public ServiceCategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ServiceCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.ServiceCategory.GetAll");

        var categories = await _unitOfWork.ServiceCategories
            .GetAllAsync(ct);

        activity?.SetTag(
            "catalog.categories.count",
            categories.Count);

        _logger.LogDebug(
            "Retrieved {CategoryCount} service categories",
            categories.Count);

        return _mapper.Map<IReadOnlyList<ServiceCategoryDto>>(
            categories);
    }

    public async Task<ServiceCategoryDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.ServiceCategory.GetById");

        activity?.SetTag("catalog.category.id", id);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Service category {CategoryId} was not found",
                id);

            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        _logger.LogDebug(
            "Retrieved service category {CategoryId}",
            id);

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task<ServiceCategoryDto> CreateAsync(
        CreateServiceCategoryDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.ServiceCategory.Create");

        ArgumentNullException.ThrowIfNull(dto);

        var category = new ServiceCategory(
            dto.CategoryName,
            dto.Description);

        await _unitOfWork.ServiceCategories.AddAsync(
            category,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);

        activity?.SetTag("catalog.category.id", category.CategoryId);

        _logger.LogInformation(
            "Created service category {CategoryId}",
            category.CategoryId);

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task<ServiceCategoryDto> UpdateAsync(
        long id,
        UpdateServiceCategoryDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.ServiceCategory.Update");

        activity?.SetTag("catalog.category.id", id);

        ArgumentNullException.ThrowIfNull(dto);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot update service category {CategoryId}: category was not found",
                id);

            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        category.Rename(dto.CategoryName);
        category.UpdateDescription(dto.Description);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Updated service category {CategoryId}",
            id);

        return _mapper.Map<ServiceCategoryDto>(category);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.ServiceCategory.Delete");

        activity?.SetTag("catalog.category.id", id);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { id }, ct);

        if (category is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot delete service category {CategoryId}: category was not found",
                id);

            throw new NotFoundException(
                $"Service category with ID {id} was not found.");
        }

        _unitOfWork.ServiceCategories.Remove(category);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deleted service category {CategoryId}",
            id);
    }
}