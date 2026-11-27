using AutoMapper;
using Appointments.Domain.Entities;
using Appointments.Bll.DTOs.Patients;
using Appointments.Bll.DTOs.Appointments;

namespace Appointments.Bll.Mapping;

public class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        // Patients
        CreateMap<Patient, PatientDto>();
        CreateMap<CreatePatientDto, Patient>();
        CreateMap<UpdatePatientDto, Patient>();

        // Appointments
        CreateMap<Appointment, AppointmentDto>();
        CreateMap<CreateAppointmentDto, Appointment>();

        // Appointment Services
        CreateMap<AppointmentService, AppointmentServiceDto>();
        CreateMap<CreateAppointmentServiceDto, AppointmentService>();
    }
}