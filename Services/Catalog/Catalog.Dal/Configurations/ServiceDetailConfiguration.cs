using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class ServiceDetailConfiguration
    : IEntityTypeConfiguration<ServiceDetail>
{
    public void Configure(EntityTypeBuilder<ServiceDetail> builder)
    {
        builder.ToTable("ServiceDetails");

        builder.HasKey(x => x.ServiceDetailId);

        builder.Property(x => x.ServiceDetailId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ServiceId)
            .IsRequired();

        builder.Property(x => x.PreparationInstructions)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(x => x.Contraindications)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.HasIndex(x => x.ServiceId)
            .IsUnique()
            .HasDatabaseName("UX_ServiceDetails_ServiceId");
    }
}