
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class MedicalServiceConfiguration
    : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.ToTable("MedicalServices", table =>
        {
            table.HasCheckConstraint(
                "CK_MedicalServices_Price",
                "[Price] >= 0");

            table.HasCheckConstraint(
                "CK_MedicalServices_DurationMinutes",
                "[DurationMinutes] > 0");
        });

        builder.HasKey(x => x.ServiceId);

        builder.Property(x => x.ServiceId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.EquipmentId)
            .IsRequired(false);

        builder.Property(x => x.ServiceName)
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        // Грошове поле: точність decimal(18,2)
        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_MedicalServices_CategoryId");

        builder.HasIndex(x => x.Price)
            .HasDatabaseName("IX_MedicalServices_Price");

        // ServiceCategory 1:N MedicalService
        builder.HasOne(x => x.Category)
            .WithMany(x => x.MedicalServices)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MedicalServices_Categories");

        // Equipment 1:N MedicalService
        builder.HasOne(x => x.Equipment)
            .WithMany(x => x.MedicalServices)
            .HasForeignKey(x => x.EquipmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_MedicalServices_Equipment");

        // MedicalService 1:1 ServiceDetail
        builder.HasOne(x => x.Detail)
            .WithOne(x => x.Service)
            .HasForeignKey<ServiceDetail>(x => x.ServiceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ServiceDetails_MedicalServices");
    }
}
