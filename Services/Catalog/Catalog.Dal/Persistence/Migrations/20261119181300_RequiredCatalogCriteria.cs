using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Dal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RequiredCatalogCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "ServicePackages",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "MedicalServices",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.InsertData(
                table: "ServiceCategories",
                columns: new[] { "CategoryId", "CategoryName", "CreatedAt", "Description" },
                values: new object[,]
                {
                    { 9001L, "Emergency Medicine", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Emergency and urgent medical services." },
                    { 9002L, "Preventive Medicine", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Preventive healthcare services." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceCategories",
                keyColumn: "CategoryId",
                keyValue: 9001L);

            migrationBuilder.DeleteData(
                table: "ServiceCategories",
                keyColumn: "CategoryId",
                keyValue: 9002L);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "ServicePackages",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "MedicalServices",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
