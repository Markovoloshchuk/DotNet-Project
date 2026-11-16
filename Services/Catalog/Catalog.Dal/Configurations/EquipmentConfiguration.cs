using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public sealed class EquipmentConfiguration
    : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.HasKey(x => x.EquipmentId);

        builder.Property(x => x.EquipmentId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.EquipmentName)
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SerialNumber)
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PurchaseDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.IsOperational)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.SerialNumber)
            .IsUnique()
            .HasDatabaseName("IX_Equipment_SerialNumber");
    }
}