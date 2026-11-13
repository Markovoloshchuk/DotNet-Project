
using Dapper;
using Microsoft.Data.SqlClient;
using Appointments.Dal.Interfaces;
using Appointments.Domain.Entities;

namespace Appointments.Dal.Repositories;

public sealed class AppointmentRepository
    : BaseDapperRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(
        SqlConnection connection,
        Func<SqlTransaction?> currentTransaction)
        : base(
            connection,
            currentTransaction,
            "dbo.Appointments",
            "AppointmentId",
            new[]
            {
                "PatientId",
                "DoctorId",
                "AppointmentDate",
                "Status",
                "TotalAmount",
                "CreatedBy"
            },
            new[]
            {
                "PatientId",
                "DoctorId",
                "AppointmentDate",
                "Status",
                "TotalAmount",
                "UpdatedAt",
                "UpdatedBy",
                "IsDeleted"
            })
    {
    }

    public override async Task<Appointment?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT *
            FROM dbo.Appointments
            WHERE AppointmentId = @AppointmentId
              AND IsDeleted = 0;
            """;

        return await Connection.QuerySingleOrDefaultAsync<Appointment>(
            new CommandDefinition(
                sql,
                new { AppointmentId = id },
                Transaction,
                cancellationToken: ct));
    }

    public override async Task<IReadOnlyList<Appointment>> GetAllAsync(
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT *
            FROM dbo.Appointments
            WHERE IsDeleted = 0;
            """;

        var result = await Connection.QueryAsync<Appointment>(
            new CommandDefinition(
                sql,
                transaction: Transaction,
                cancellationToken: ct));

        return result.AsList();
    }

    public async Task<Appointment?> GetWithServicesAsync(
        long appointmentId,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT
                a.AppointmentId,
                a.PatientId,
                a.DoctorId,
                a.AppointmentDate,
                a.Status,
                a.TotalAmount,
                a.CreatedAt,
                a.CreatedBy,
                a.UpdatedAt,
                a.UpdatedBy,
                a.IsDeleted,
                a.RowVersion,

                s.AppointmentServiceId,
                s.AppointmentId,
                s.ServiceId,
                s.ServiceNameSnapshot,
                s.PriceSnapshot,
                s.Quantity

            FROM dbo.Appointments AS a

            LEFT JOIN dbo.AppointmentServices AS s
                ON a.AppointmentId = s.AppointmentId

            WHERE a.AppointmentId = @AppointmentId
              AND a.IsDeleted = 0;
            """;

        Appointment? result = null;

        await Connection.QueryAsync<Appointment, AppointmentService?, Appointment>(
            new CommandDefinition(
                sql,
                new { AppointmentId = appointmentId },
                Transaction,
                cancellationToken: ct),
            (appointment, service) =>
            {
                result ??= appointment;

                if (service is not null &&
                    service.AppointmentServiceId > 0)
                {
                    result.Services.Add(service);
                }

                return result;
            },
            splitOn: "AppointmentServiceId");

        return result;
    }

    public async Task<IReadOnlyList<Appointment>> GetAllWithServicesAsync(
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT
                a.AppointmentId,
                a.PatientId,
                a.DoctorId,
                a.AppointmentDate,
                a.Status,
                a.TotalAmount,
                a.CreatedAt,
                a.CreatedBy,
                a.UpdatedAt,
                a.UpdatedBy,
                a.IsDeleted,
                a.RowVersion,

                s.AppointmentServiceId,
                s.AppointmentId,
                s.ServiceId,
                s.ServiceNameSnapshot,
                s.PriceSnapshot,
                s.Quantity

            FROM dbo.Appointments AS a

            LEFT JOIN dbo.AppointmentServices AS s
                ON a.AppointmentId = s.AppointmentId

            WHERE a.IsDeleted = 0

            ORDER BY a.AppointmentDate, a.AppointmentId;
            """;

        var appointments = new Dictionary<long, Appointment>();

        await Connection.QueryAsync<Appointment, AppointmentService?, Appointment>(
            new CommandDefinition(
                sql,
                transaction: Transaction,
                cancellationToken: ct),
            (appointment, service) =>
            {
                if (!appointments.TryGetValue(
                    appointment.AppointmentId,
                    out var existingAppointment))
                {
                    existingAppointment = appointment;
                    appointments.Add(
                        appointment.AppointmentId,
                        existingAppointment);
                }

                if (service is not null &&
                    service.AppointmentServiceId > 0)
                {
                    existingAppointment.Services.Add(service);
                }

                return existingAppointment;
            },
            splitOn: "AppointmentServiceId");

        return appointments.Values.ToList();
    }

    public override async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            UPDATE dbo.Appointments
            SET IsDeleted = 1,
                UpdatedAt = GETDATE()
            WHERE AppointmentId = @AppointmentId
              AND IsDeleted = 0;
            """;

        var affectedRows = await Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { AppointmentId = id },
                Transaction,
                cancellationToken: ct));

        return affectedRows > 0;
    }
}
