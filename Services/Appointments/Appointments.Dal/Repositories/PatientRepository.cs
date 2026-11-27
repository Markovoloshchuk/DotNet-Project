using System.Data;
using Microsoft.Data.SqlClient;
using Appointments.Dal.Interfaces;
using Appointments.Domain.Entities;

namespace Appointments.Dal.Repositories;

public sealed class PatientRepository : RepositoryBase, IPatientRepository
{
    public PatientRepository(
        SqlConnection connection,
        Func<SqlTransaction?> currentTransaction)
        : base(connection, currentTransaction)
    {
    }

    public async Task<Patient?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT PatientId, FullName, PhoneNumber,
                   Email, DateOfBirth, CreatedAt
            FROM dbo.Patients
            WHERE PatientId = @PatientId;
            """;

        await using var command = new SqlCommand(sql, Connection, Transaction);

        command.Parameters.Add("@PatientId", SqlDbType.BigInt).Value = id;

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return MapPatient(reader);
    }

    public async Task<IReadOnlyList<Patient>> GetAllAsync(
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            SELECT PatientId, FullName, PhoneNumber,
                   Email, DateOfBirth, CreatedAt
            FROM dbo.Patients
            ORDER BY PatientId;
            """;

        await using var command = new SqlCommand(sql, Connection, Transaction);

        await using var reader = await command.ExecuteReaderAsync(ct);

        var patients = new List<Patient>();

        while (await reader.ReadAsync(ct))
        {
            patients.Add(MapPatient(reader));
        }

        return patients;
    }

    public async Task<long> AddAsync(
        Patient patient,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            INSERT INTO dbo.Patients
                (FullName, PhoneNumber, Email, DateOfBirth)
            OUTPUT INSERTED.PatientId
            VALUES
                (@FullName, @PhoneNumber, @Email, @DateOfBirth);
            """;

        await using var command = new SqlCommand(sql, Connection, Transaction);

        AddPatientParameters(command, patient);

        var result = await command.ExecuteScalarAsync(ct);

        return Convert.ToInt64(result);
    }

    public async Task<bool> UpdateAsync(
        Patient patient,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            UPDATE dbo.Patients
            SET FullName = @FullName,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                DateOfBirth = @DateOfBirth
            WHERE PatientId = @PatientId;
            """;

        await using var command = new SqlCommand(sql, Connection, Transaction);

        AddPatientParameters(command, patient);

        command.Parameters.Add("@PatientId", SqlDbType.BigInt)
            .Value = patient.PatientId;

        var affectedRows = await command.ExecuteNonQueryAsync(ct);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken ct = default)
    {
        await EnsureOpenAsync(ct);

        const string sql = """
            DELETE FROM dbo.Patients
            WHERE PatientId = @PatientId;
            """;

        await using var command = new SqlCommand(sql, Connection, Transaction);

        command.Parameters.Add("@PatientId", SqlDbType.BigInt).Value = id;

        var affectedRows = await command.ExecuteNonQueryAsync(ct);

        return affectedRows > 0;
    }

    private static void AddPatientParameters(
        SqlCommand command,
        Patient patient)
    {
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150)
            .Value = patient.FullName;

        command.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 20)
            .Value = patient.PhoneNumber;

        command.Parameters.Add("@Email", SqlDbType.NVarChar, 100)
            .Value = (object?)patient.Email ?? DBNull.Value;

        command.Parameters.Add("@DateOfBirth", SqlDbType.Date)
            .Value = patient.DateOfBirth.Date;
    }

    private static Patient MapPatient(SqlDataReader reader)
    {
        var emailOrdinal = reader.GetOrdinal("Email");

        return new Patient
        {
            PatientId = reader.GetInt64(reader.GetOrdinal("PatientId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
            Email = reader.IsDBNull(emailOrdinal)
                ? null
                : reader.GetString(emailOrdinal),
            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}