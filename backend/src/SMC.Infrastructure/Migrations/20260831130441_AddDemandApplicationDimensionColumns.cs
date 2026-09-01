using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDemandApplicationDimensionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedRate",
                table: "DemandApplications",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthFt",
                table: "DemandApplications",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthFt",
                table: "DemandApplications",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculatedRate",
                table: "DemandApplications");

            migrationBuilder.DropColumn(
                name: "LengthFt",
                table: "DemandApplications");

            migrationBuilder.DropColumn(
                name: "WidthFt",
                table: "DemandApplications");
        }
    }
}
