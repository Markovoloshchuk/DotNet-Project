
using System.Data;
using Catalog.Dal.Interfaces;
using Catalog.Domain.Entities;

namespace Catalog.Dal.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<ServiceCategory> ServiceCategories { get; }

    IGenericRepository<Equipment> Equipment { get; }

    IGenericRepository<MedicalService> MedicalServices { get; }

    IGenericRepository<ServiceDetail> ServiceDetails { get; }

    IGenericRepository<ServicePackage> ServicePackages { get; }

    IGenericRepository<PackageService> PackageServices { get; }

    IMedicalServiceRepository MedicalServiceQueries { get; }

    Task<int> SaveChangesAsync(
        CancellationToken ct = default);

    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default);

    Task CommitTransactionAsync(
        CancellationToken ct = default);

    Task RollbackTransactionAsync(
        CancellationToken ct = default);
}
