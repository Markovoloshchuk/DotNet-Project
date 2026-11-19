
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class ServiceCategoryConfiguration
    : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("ServiceCategories");

        builder.HasKey(x => x.CategoryId);

        builder.Property(x => x.CategoryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryName)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");

        builder.HasIndex(x => x.CategoryName)
            .IsUnique()
            .HasDatabaseName("UX_ServiceCategories_CategoryName");

        // Static reference data with fixed IDs.
        builder.HasData(
            new
            {
                CategoryId = 9001L,
                CategoryName = "Emergency Medicine",
                Description = "Emergency and urgent medical services.",
                CreatedAt = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                CategoryId = 9002L,
                CategoryName = "Preventive Medicine",
                Description = "Preventive healthcare services.",
                CreatedAt = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
