using System.Data;
using AutoMapper;
using Appointments.Bll.DTOs.Appointments;
using Appointments.Bll.Interfaces;
using Appointments.Dal.UnitOfWork;
using Appointments.Domain.Constants;
using Appointments.Domain.Entities;
using Appointments.Domain.Exceptions;

namespace Appointments.Bll.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AppointmentDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        var appointment = await _unitOfWork.Appointments
            .GetWithServicesAsync(id, ct);

        if (appointment is null)
            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");

        return _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var appointments = await _unitOfWork.Appointments
            .GetAllWithServicesAsync(ct);

        return _mapper.Map<IReadOnlyList<AppointmentDto>>(appointments);
    }

    public async Task<AppointmentDto> CreateAsync(
        CreateAppointmentDto dto,
        CancellationToken ct = default)
    {
        ValidateAppointmentDate(dto.AppointmentDate);
        ValidateServices(dto.Services);

        var patient = await _unitOfWork.Patients
            .GetByIdAsync(dto.PatientId, ct);

        if (patient is null)
            throw new NotFoundException(
                $"Patient with ID {dto.PatientId} was not found.");

        var totalAmount = dto.Services.Sum(
            service => service.PriceSnapshot * service.Quantity);

        if (totalAmount > 99_999_999.99m)
            throw new ValidationException(
                "Total appointment amount exceeds the allowed limit.");

        await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, ct);

        try
        {
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                Status = AppointmentStatuses.Scheduled,
                TotalAmount = totalAmount,
                CreatedBy = "API"
            };

            var appointmentId = await _unitOfWork.Appointments
                .AddAsync(appointment, ct);

            foreach (var serviceDto in dto.Services)
            {
                var service = new Appointments.Domain.Entities.AppointmentService
                {
                    AppointmentId = appointmentId,
                    ServiceId = serviceDto.ServiceId,
                    ServiceNameSnapshot = serviceDto.ServiceNameSnapshot,
                    PriceSnapshot = serviceDto.PriceSnapshot,
                    Quantity = serviceDto.Quantity
                };

                await _unitOfWork.AppointmentServices
                    .AddAsync(service, ct);
            }

            var createdAppointment = await _unitOfWork.Appointments
                .GetWithServicesAsync(appointmentId, ct);

            if (createdAppointment is null)
                throw new NotFoundException(
                    $"Appointment with ID {appointmentId} was not found.");

            await _unitOfWork.CommitAsync(ct);

            return _mapper.Map<AppointmentDto>(createdAppointment);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<AppointmentDto> UpdateAsync(
        long id,
        UpdateAppointmentDto dto,
        CancellationToken ct = default)
    {
        ValidateAppointmentDate(dto.AppointmentDate);

        var existing = await _unitOfWork.Appointments
            .GetByIdAsync(id, ct);

        if (existing is null)
            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");

        var patient = await _unitOfWork.Patients
            .GetByIdAsync(dto.PatientId, ct);

        if (patient is null)
            throw new NotFoundException(
                $"Patient with ID {dto.PatientId} was not found.");

        var appointment = new Appointment
        {
            AppointmentId = id,
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate,
            Status = existing.Status,
            TotalAmount = existing.TotalAmount,
            CreatedAt = existing.CreatedAt,
            CreatedBy = existing.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "API",
            IsDeleted = existing.IsDeleted,
            RowVersion = existing.RowVersion
        };

        var updated = await _unitOfWork.Appointments
            .UpdateAsync(appointment, ct);

        if (!updated)
            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        var deleted = await _unitOfWork.Appointments
            .DeleteAsync(id, ct);

        if (!deleted)
            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");
    }

    private static void ValidateAppointmentDate(DateTime date)
    {
        if (date == default || date <= DateTime.UtcNow)
        {
            throw new ValidationException(
                "Appointment date must be in the future.");
        }
    }

    private static void ValidateServices(
        IReadOnlyCollection<CreateAppointmentServiceDto>? services)
    {
        if (services is null || services.Count == 0)
            throw new ValidationException(
                "An appointment must contain at least one service.");

        foreach (var service in services)
        {
            if (service.ServiceId <= 0)
                throw new ValidationException(
                    "Service ID must be greater than zero.");

            if (string.IsNullOrWhiteSpace(service.ServiceNameSnapshot))
                throw new ValidationException(
                    "Service name cannot be empty.");

            if (service.PriceSnapshot < 0)
                throw new ValidationException(
                    "Service price cannot be negative.");

            if (service.Quantity <= 0)
                throw new ValidationException(
                    "Service quantity must be greater than zero.");
        }
    }
}