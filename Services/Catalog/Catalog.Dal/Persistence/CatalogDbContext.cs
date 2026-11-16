using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Persistence;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<ServiceCategory> ServiceCategories =>
        Set<ServiceCategory>();

    public DbSet<Equipment> Equipment =>
        Set<Equipment>();

    public DbSet<MedicalService> MedicalServices =>
        Set<MedicalService>();

    public DbSet<ServiceDetail> ServiceDetails =>
        Set<ServiceDetail>();

    public DbSet<ServicePackage> ServicePackages =>
        Set<ServicePackage>();

    public DbSet<PackageService> PackageServices =>
        Set<PackageService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly);
    }
}