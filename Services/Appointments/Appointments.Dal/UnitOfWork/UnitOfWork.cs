using System.Data;
using Microsoft.Data.SqlClient;
using Appointments.Dal.Interfaces;
using Appointments.Dal.Repositories;

namespace Appointments.Dal.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;

    public IPatientRepository Patients { get; }

    public IAppointmentRepository Appointments { get; }

    public IAppointmentServiceRepository AppointmentServices { get; }

    public UnitOfWork(string connectionString)
    {
        _connection = new SqlConnection(connectionString);

        Patients = new PatientRepository(
            _connection,
            () => _transaction);

        Appointments = new AppointmentRepository(
            _connection,
            () => _transaction);

        AppointmentServices = new AppointmentServiceRepository(
            _connection,
            () => _transaction);
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException(
                "A transaction is already active.");

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);

        _transaction = (SqlTransaction)await _connection
            .BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        var transaction = _transaction
            ?? throw new InvalidOperationException(
                "There is no active transaction.");

        try
        {
            await transaction.CommitAsync(ct);
        }
        finally
        {
            await transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        var transaction = _transaction;

        if (transaction is null)
            return;

        try
        {
            await transaction.RollbackAsync(ct);
        }
        finally
        {
            await transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_transaction is not null)
                await RollbackAsync();
        }
        finally
        {
            await _connection.DisposeAsync();
        }
    }
}