
using System.Data;
using Catalog.Dal.Interfaces;
using Catalog.Dal.Persistence;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Catalog.Dal.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _context;

    private IDbContextTransaction? _transaction;

    public IGenericRepository<ServiceCategory> ServiceCategories
        { get; }

    public IGenericRepository<Equipment> Equipment
        { get; }

    public IGenericRepository<MedicalService> MedicalServices
        { get; }

    public IGenericRepository<ServiceDetail> ServiceDetails
        { get; }

    public IGenericRepository<ServicePackage> ServicePackages
        { get; }

    public IGenericRepository<PackageService> PackageServices
        { get; }

    public UnitOfWork(
        CatalogDbContext context,
        IGenericRepository<ServiceCategory> serviceCategories,
        IGenericRepository<Equipment> equipment,
        IGenericRepository<MedicalService> medicalServices,
        IGenericRepository<ServiceDetail> serviceDetails,
        IGenericRepository<ServicePackage> servicePackages,
        IGenericRepository<PackageService> packageServices)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;

        ServiceCategories = serviceCategories;
        Equipment = equipment;
        MedicalServices = medicalServices;
        ServiceDetails = serviceDetails;
        ServicePackages = servicePackages;
        PackageServices = packageServices;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException(
                "A transaction is already active.");
        }

        _transaction = await _context.Database
            .BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitTransactionAsync(
        CancellationToken ct = default)
    {
        var transaction = _transaction
            ?? throw new InvalidOperationException(
                "There is no active transaction.");

        try
        {
            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);
            }
            catch
            {
                // Preserve the original exception.
            }

            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(
        CancellationToken ct = default)
    {
        var transaction = _transaction;

        if (transaction is null)
        {
            return;
        }

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
        if (_transaction is not null)
        {
            await RollbackTransactionAsync();
        }
    }
}
