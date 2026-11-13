using Dapper;
using Microsoft.Data.SqlClient;
using Appointments.Dal.Interfaces;
using Appointments.Domain.Entities;

namespace Appointments.Dal.Repositories;

public sealed class AppointmentServiceRepository
    : BaseDapperRepository<AppointmentService>,
      IAppointmentServiceRepository
{
    public AppointmentServiceRepository(
        SqlConnection connection,
        Func<SqlTransaction?> currentTransaction)
        : base(
            connection,
            currentTransaction,
            "dbo.AppointmentServices",
            "AppointmentServiceId",
            new[]
            {
                "AppointmentId",
                "ServiceId",
                "ServiceNameSnapshot",
                "PriceSnapshot",
                "Quantity"
            },
            new[]
            {
                "ServiceId",
                "ServiceNameSnapshot",
                "PriceSnapshot",
                "Quantity"
            })
    {
    }

    public async Task<IReadOnlyList<AppointmentService>> GetByAppointmentIdAsync(
        long appointmentId,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT *
            FROM dbo.AppointmentServices
            WHERE AppointmentId = @AppointmentId;
            """;

        var result = await Connection.QueryAsync<AppointmentService>(
            new CommandDefinition(
                sql,
                new { AppointmentId = appointmentId },
                Transaction,
                cancellationToken: ct));

        return result.AsList();
    }
}