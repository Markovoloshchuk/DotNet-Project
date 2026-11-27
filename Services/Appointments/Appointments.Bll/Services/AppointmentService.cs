using System.Data;
using System.Diagnostics;
using AutoMapper;
using Appointments.Bll.DTOs.Appointments;
using Appointments.Bll.Interfaces;
using Appointments.Dal.UnitOfWork;
using Appointments.Domain.Constants;
using Appointments.Domain.Entities;
using Appointments.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Appointments.Bll.Services;

public sealed class AppointmentService : IAppointmentService
{
    private static readonly ActivitySource ActivitySource =
        new("Clinic.Appointments");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppointmentDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Appointment.GetById",
            ActivityKind.Internal);

        activity?.SetTag("appointment.id", id);

        var appointment = await _unitOfWork.Appointments
            .GetWithServicesAsync(id, ct);

        if (appointment is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Appointment {AppointmentId} was not found",
                id);

            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");
        }

        _logger.LogDebug(
            "Retrieved appointment {AppointmentId}",
            id);

        return _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Appointment.GetAll",
            ActivityKind.Internal);

        var appointments = await _unitOfWork.Appointments
            .GetAllWithServicesAsync(ct);

        _logger.LogDebug(
            "Retrieved {AppointmentCount} appointments",
            appointments.Count);

        activity?.SetTag(
            "appointments.count",
            appointments.Count);

        return _mapper.Map<IReadOnlyList<AppointmentDto>>(appointments);
    }

    public async Task<AppointmentDto> CreateAsync(
        CreateAppointmentDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Appointment.Create",
            ActivityKind.Internal);

        activity?.SetTag("patient.id", dto.PatientId);
        activity?.SetTag("doctor.id", dto.DoctorId);

        _logger.LogInformation(
            "Creating appointment for patient {PatientId} and doctor {DoctorId}",
            dto.PatientId,
            dto.DoctorId);

        ValidateAppointmentDate(dto.AppointmentDate);

        var services = dto.Services
            ?? throw new ValidationException(
                "An appointment must contain at least one service.");

        ValidateServices(services);

        activity?.SetTag(
            "appointment.services.count",
            services.Count);

        var patient = await _unitOfWork.Patients
            .GetByIdAsync(dto.PatientId, ct);

        if (patient is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot create appointment because patient {PatientId} was not found",
                dto.PatientId);

            throw new NotFoundException(
                $"Patient with ID {dto.PatientId} was not found.");
        }

        var totalAmount = services.Sum(
            service => service.PriceSnapshot * service.Quantity);

        activity?.SetTag(
            "appointment.total_amount",
            (double)totalAmount);

        if (totalAmount > 99_999_999.99m)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            throw new ValidationException(
                "Total appointment amount exceeds the allowed limit.");
        }

        await _unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            ct);

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

            foreach (var serviceDto in services)
            {
                var service =
                    new Appointments.Domain.Entities.AppointmentService
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
            {
                throw new NotFoundException(
                    $"Appointment with ID {appointmentId} was not found.");
            }

            await _unitOfWork.CommitAsync(ct);

            activity?.SetTag("appointment.id", appointmentId);

            _logger.LogInformation(
                "Created appointment {AppointmentId} for patient {PatientId} and doctor {DoctorId} with {ServiceCount} services",
                appointmentId,
                dto.PatientId,
                dto.DoctorId,
                services.Count);

            return _mapper.Map<AppointmentDto>(createdAppointment);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(CancellationToken.None);

            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogError(
                ex,
                "Failed to create appointment for patient {PatientId} and doctor {DoctorId}",
                dto.PatientId,
                dto.DoctorId);

            throw;
        }
    }

    public async Task<AppointmentDto> UpdateAsync(
        long id,
        UpdateAppointmentDto dto,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Appointment.Update",
            ActivityKind.Internal);

        activity?.SetTag("appointment.id", id);
        activity?.SetTag("patient.id", dto.PatientId);
        activity?.SetTag("doctor.id", dto.DoctorId);

        _logger.LogInformation(
            "Updating appointment {AppointmentId}",
            id);

        ValidateAppointmentDate(dto.AppointmentDate);

        var existing = await _unitOfWork.Appointments
            .GetByIdAsync(id, ct);

        if (existing is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot update appointment {AppointmentId} because it was not found",
                id);

            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");
        }

        var patient = await _unitOfWork.Patients
            .GetByIdAsync(dto.PatientId, ct);

        if (patient is null)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot update appointment {AppointmentId} because patient {PatientId} was not found",
                id,
                dto.PatientId);

            throw new NotFoundException(
                $"Patient with ID {dto.PatientId} was not found.");
        }

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
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Appointment {AppointmentId} was not updated",
                id);

            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");
        }

        var result = await GetByIdAsync(id, ct);

        _logger.LogInformation(
            "Updated appointment {AppointmentId}",
            id);

        return result;
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        using var activity = ActivitySource.StartActivity(
            "Appointment.Delete",
            ActivityKind.Internal);

        activity?.SetTag("appointment.id", id);

        _logger.LogInformation(
            "Deleting appointment {AppointmentId}",
            id);

        var deleted = await _unitOfWork.Appointments
            .DeleteAsync(id, ct);

        if (!deleted)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            _logger.LogWarning(
                "Cannot delete appointment {AppointmentId} because it was not found",
                id);

            throw new NotFoundException(
                $"Appointment with ID {id} was not found.");
        }

        _logger.LogInformation(
            "Deleted appointment {AppointmentId}",
            id);
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
        {
            throw new ValidationException(
                "An appointment must contain at least one service.");
        }

        foreach (var service in services)
        {
            if (service.ServiceId <= 0)
            {
                throw new ValidationException(
                    "Service ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(service.ServiceNameSnapshot))
            {
                throw new ValidationException(
                    "Service name cannot be empty.");
            }

            if (service.PriceSnapshot < 0)
            {
                throw new ValidationException(
                    "Service price cannot be negative.");
            }

            if (service.Quantity <= 0)
            {
                throw new ValidationException(
                    "Service quantity must be greater than zero.");
            }
        }
    }
}