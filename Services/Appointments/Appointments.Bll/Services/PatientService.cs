using AutoMapper;
using Appointments.Bll.DTOs.Patients;
using Appointments.Bll.Interfaces;
using Appointments.Dal.UnitOfWork;
using Appointments.Domain.Entities;
using Appointments.Domain.Exceptions;

namespace Appointments.Bll.Services;

public sealed class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PatientService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PatientDto> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(id, ct);

        if (patient is null)
            throw new NotFoundException($"Patient with ID {id} was not found.");

        return _mapper.Map<PatientDto>(patient);
    }

    public async Task<IReadOnlyList<PatientDto>> GetAllAsync(
        CancellationToken ct = default)
    {
        var patients = await _unitOfWork.Patients.GetAllAsync(ct);

        return _mapper.Map<IReadOnlyList<PatientDto>>(patients);
    }

    public async Task<PatientDto> CreateAsync(
        CreatePatientDto dto,
        CancellationToken ct = default)
    {
        ValidateDateOfBirth(dto.DateOfBirth);

        var patient = _mapper.Map<Patient>(dto);

        var patientId = await _unitOfWork.Patients.AddAsync(patient, ct);

        return await GetByIdAsync(patientId, ct);
    }

    public async Task<PatientDto> UpdateAsync(
        long id,
        UpdatePatientDto dto,
        CancellationToken ct = default)
    {
        var existingPatient = await _unitOfWork.Patients.GetByIdAsync(id, ct);

        if (existingPatient is null)
            throw new NotFoundException($"Patient with ID {id} was not found.");

        ValidateDateOfBirth(dto.DateOfBirth);

        var patient = _mapper.Map<Patient>(dto);

        patient.PatientId = id;
        patient.CreatedAt = existingPatient.CreatedAt;

        var updated = await _unitOfWork.Patients.UpdateAsync(patient, ct);

        if (!updated)
            throw new NotFoundException($"Patient with ID {id} was not found.");

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        var deleted = await _unitOfWork.Patients.DeleteAsync(id, ct);

        if (!deleted)
            throw new NotFoundException($"Patient with ID {id} was not found.");
    }

    private static void ValidateDateOfBirth(DateTime dateOfBirth)
    {
        if (dateOfBirth == default || dateOfBirth.Date > DateTime.UtcNow.Date)
        {
            throw new ValidationException(
                "Date of birth must be a valid date in the past.");
        }
    }
}