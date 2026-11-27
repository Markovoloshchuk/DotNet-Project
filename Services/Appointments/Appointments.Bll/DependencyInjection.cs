using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Appointments.Bll.Interfaces;
using Appointments.Bll.Mapping;
using Appointments.Bll.Services;
using Appointments.Dal.UnitOfWork;

namespace Appointments.Bll;

public static class DependencyInjection
{
    public static IServiceCollection AddAppointmentsBll(
        this IServiceCollection services,
        string connectionString,
        string? autoMapperLicenseKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Unit of Work: один екземпляр на HTTP-запит
        services.AddScoped<IUnitOfWork>(
            _ => new UnitOfWork(connectionString));

        // Business services
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        // AutoMapper
        services.AddAutoMapper(
            cfg =>
            {
                if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
                    cfg.LicenseKey = autoMapperLicenseKey;
            },
            typeof(AppointmentMappingProfile));

        return services;
    }
}