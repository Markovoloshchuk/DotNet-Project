using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class ServicePackageConfiguration
    : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("ServicePackages", table =>
        {
            table.HasCheckConstraint(
                "CK_ServicePackages_TotalPrice",
                "[TotalPrice] >= 0");

            table.HasCheckConstraint(
                "CK_ServicePackages_DiscountPercentage",
                "[DiscountPercentage] >= 0 AND [DiscountPercentage] <= 100");
        });

        builder.HasKey(x => x.PackageId);

        builder.Property(x => x.PackageId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.PackageName)
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(x => x.DiscountPercentage)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0.00m)
            .IsRequired();
    }
}