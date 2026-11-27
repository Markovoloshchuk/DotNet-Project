using System.Diagnostics;
using AutoMapper;
using Catalog.Bll.DTOs.Common;
using Catalog.Bll.DTOs.MedicalServices;
using Catalog.Bll.Interfaces;
using Catalog.Dal.UnitOfWork;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Catalog.Bll.Services;

public sealed class MedicalServiceService : IMedicalServiceService
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Catalog");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MedicalServiceService> _logger;

    public MedicalServiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MedicalServiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MedicalServiceDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.GetAll");

        var services = await _unitOfWork.MedicalServiceQueries
            .GetAllWithDetailsAsync(ct);

        activity?.SetTag("catalog.services.count", services.Count);

        _logger.LogDebug(
            "Retrieved {ServiceCount} medical services",
            services.Count);

        return _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
            services);
    }

    public async Task<MedicalServiceDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.GetById");

        activity?.SetTag("catalog.service.id", id);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var service = await _unitOfWork.MedicalServiceQueries
            .GetByIdWithDetailsAsync(id, ct);

        if (service is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Medical service {ServiceId} was not found",
                id);

            throw new NotFoundException(
                $"Medical service with ID {id} was not found.");
        }

        _logger.LogDebug(
            "Retrieved medical service {ServiceId}",
            id);

        return _mapper.Map<MedicalServiceDto>(service);
    }

    public async Task<PagedResultDto<MedicalServiceDto>> SearchAsync(
        SearchMedicalServicesDto filter,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.Search");

        ArgumentNullException.ThrowIfNull(filter);

        activity?.SetTag(
            "catalog.page.number",
            filter.PageNumber);

        activity?.SetTag(
            "catalog.page.size",
            filter.PageSize);

        var result = await _unitOfWork.MedicalServiceQueries
            .SearchAsync(
                name: filter.Name,
                categoryId: filter.CategoryId,
                minPrice: filter.MinPrice,
                maxPrice: filter.MaxPrice,
                isActive: filter.IsActive,
                pageNumber: filter.PageNumber,
                pageSize: filter.PageSize,
                sortBy: filter.SortBy,
                sortDescending: filter.SortDescending,
                ct: ct);

        activity?.SetTag(
            "catalog.search.result_count",
            result.Items.Count);

        _logger.LogDebug(
            "Medical service search returned {ResultCount} results",
            result.Items.Count);

        return new PagedResultDto<MedicalServiceDto>
        {
            Items = _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
                result.Items),

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<IReadOnlyList<MedicalServiceDto>>
        GetTopByPriceAsync(
            int count,
            CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.GetTopByPrice");

        activity?.SetTag("catalog.services.requested_count", count);

        var services = await _unitOfWork.MedicalServiceQueries
            .GetTopByPriceAsync(count, ct);

        activity?.SetTag("catalog.services.result_count", services.Count);

        _logger.LogDebug(
            "Retrieved {ResultCount} top-priced medical services",
            services.Count);

        return _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
            services);
    }

    public async Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.Create");

        ArgumentNullException.ThrowIfNull(dto);

        activity?.SetTag("catalog.category.id", dto.CategoryId);
        activity?.SetTag("catalog.equipment.id", dto.EquipmentId);

        await EnsureReferencesExistAsync(
            dto.CategoryId,
            dto.EquipmentId,
            ct);

        var service = new MedicalService(
            dto.CategoryId,
            dto.ServiceName,
            dto.Price,
            dto.DurationMinutes,
            dto.EquipmentId);

        await _unitOfWork.MedicalServices.AddAsync(service, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        activity?.SetTag("catalog.service.id", service.ServiceId);

        var result = await GetByIdAsync(service.ServiceId, ct);

        _logger.LogInformation(
            "Created medical service {ServiceId} in category {CategoryId}",
            service.ServiceId,
            dto.CategoryId);

        return result;
    }

    public async Task<MedicalServiceDto> UpdateAsync(
        long id,
        UpdateMedicalServiceDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.Update");

        activity?.SetTag("catalog.service.id", id);

        ArgumentNullException.ThrowIfNull(dto);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var service = await _unitOfWork.MedicalServices
            .GetByIdAsync(new object[] { id }, ct);

        if (service is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot update medical service {ServiceId}: service was not found",
                id);

            throw new NotFoundException(
                $"Medical service with ID {id} was not found.");
        }

        await EnsureReferencesExistAsync(
            dto.CategoryId,
            dto.EquipmentId,
            ct);

        service.ChangeCategory(dto.CategoryId);
        service.AssignEquipment(dto.EquipmentId);
        service.Rename(dto.ServiceName);
        service.ChangePrice(dto.Price);
        service.ChangeDuration(dto.DurationMinutes);

        if (dto.IsActive)
        {
            service.Activate();
        }
        else
        {
            service.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var result = await GetByIdAsync(id, ct);

        _logger.LogInformation(
            "Updated medical service {ServiceId}",
            id);

        return result;
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Catalog.MedicalService.Delete");

        activity?.SetTag("catalog.service.id", id);

        if (id <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var service = await _unitOfWork.MedicalServices
            .GetByIdAsync(new object[] { id }, ct);

        if (service is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot delete medical service {ServiceId}: service was not found",
                id);

            throw new NotFoundException(
                $"Medical service with ID {id} was not found.");
        }

        _unitOfWork.MedicalServices.Remove(service);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deleted medical service {ServiceId}",
            id);
    }

    private async Task EnsureReferencesExistAsync(
        long categoryId,
        long? equipmentId,
        CancellationToken ct)
    {
        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId));
        }

        var category = await _unitOfWork.ServiceCategories
            .GetByIdAsync(new object[] { categoryId }, ct);

        if (category is null)
        {
            throw new NotFoundException(
                $"Service category with ID {categoryId} was not found.");
        }

        if (!equipmentId.HasValue)
        {
            return;
        }

        if (equipmentId.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(equipmentId));
        }

        var equipment = await _unitOfWork.Equipment
            .GetByIdAsync(
                new object[] { equipmentId.Value },
                ct);

        if (equipment is null)
        {
            throw new NotFoundException(
                $"Equipment with ID {equipmentId} was not found.");
        }
    }
}