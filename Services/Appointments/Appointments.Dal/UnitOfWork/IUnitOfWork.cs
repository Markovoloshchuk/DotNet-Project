using System.Data;
using Appointments.Dal.Interfaces;

namespace Appointments.Dal.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IPatientRepository Patients { get; }

    IAppointmentRepository Appointments { get; }

    IAppointmentServiceRepository AppointmentServices { get; }

    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    );

    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);
}