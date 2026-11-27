using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class PackageServiceConfiguration
    : IEntityTypeConfiguration<PackageService>
{
    public void Configure(EntityTypeBuilder<PackageService> builder)
    {
        builder.ToTable("PackageServices", table =>
        {
            table.HasCheckConstraint(
                "CK_PackageServices_Quantity",
                "[Quantity] > 0");
        });

        // Складений первинний ключ
        builder.HasKey(x => new { x.PackageId, x.ServiceId })
            .HasName("PK_PackageServices");

        builder.Property(x => x.PackageId)
            .IsRequired();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.CustomNotes)
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsRequired(false);

        // ServicePackage 1:N PackageService
        builder.HasOne(x => x.Package)
            .WithMany(x => x.PackageServices)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PackageServices_Packages");

        // MedicalService 1:N PackageService
        builder.HasOne(x => x.Service)
            .WithMany(x => x.PackageServices)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PackageServices_Services");
    }
}