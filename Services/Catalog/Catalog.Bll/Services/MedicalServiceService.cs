
using AutoMapper;
using Catalog.Bll.DTOs.MedicalServices;
using Catalog.Bll.Interfaces;
using Catalog.Dal.UnitOfWork;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Bll.Services;

public sealed class MedicalServiceService : IMedicalServiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MedicalServiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MedicalServiceDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var services = await _unitOfWork.MedicalServiceQueries
            .GetAllWithDetailsAsync(ct);

        return _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
            services);
    }

    public async Task<MedicalServiceDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var service = await _unitOfWork.MedicalServiceQueries
            .GetByIdWithDetailsAsync(id, ct);

        if (service is null)
        {
            throw new NotFoundException(
                $"Medical service with ID {id} was not found.");
        }

        return _mapper.Map<MedicalServiceDto>(service);
    }

    public async Task<IReadOnlyList<MedicalServiceDto>> SearchAsync(
        SearchMedicalServicesDto filter,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var services = await _unitOfWork.MedicalServiceQueries
            .SearchAsync(
                filter.Name,
                filter.CategoryId,
                filter.MinPrice,
                filter.MaxPrice,
                filter.IsActive,
                ct);

        return _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
            services);
    }

    public async Task<IReadOnlyList<MedicalServiceDto>>
        GetTopByPriceAsync(
            int count,
            CancellationToken ct = default)
    {
        var services = await _unitOfWork.MedicalServiceQueries
            .GetTopByPriceAsync(count, ct);

        return _mapper.Map<IReadOnlyList<MedicalServiceDto>>(
            services);
    }

    public async Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto dto,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

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

        return await GetByIdAsync(service.ServiceId, ct);
    }

    public async Task<MedicalServiceDto> UpdateAsync(
        long id,
        UpdateMedicalServiceDto dto,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var service = await _unitOfWork.MedicalServices
            .GetByIdAsync(new object[] { id }, ct);

        if (service is null)
        {
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
            service.Activate();
        else
            service.Deactivate();

        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));

        var service = await _unitOfWork.MedicalServices
            .GetByIdAsync(new object[] { id }, ct);

        if (service is null)
        {
            throw new NotFoundException(
                $"Medical service with ID {id} was not found.");
        }

        _unitOfWork.MedicalServices.Remove(service);

        await _unitOfWork.SaveChangesAsync(ct);
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
            return;

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
