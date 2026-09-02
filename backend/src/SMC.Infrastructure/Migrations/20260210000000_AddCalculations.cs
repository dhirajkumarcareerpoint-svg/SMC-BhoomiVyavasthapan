using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260210000000_AddCalculations")]
    public partial class AddCalculations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Calculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PeriodMonths = table.Column<int>(type: "int", nullable: false),
                    PreviousOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentDemand = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Shera = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculations", x => x.Id);
                    table.ForeignKey("FK_Calculations_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                });

            migrationBuilder.AddColumn<int>(
                name: "CalculationId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Calculations_CalculationId",
                table: "Documents",
                column: "CalculationId",
                principalTable: "Calculations",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(name: "IX_Calculations_PropertyId", table: "Calculations", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_Calculations_Status", table: "Calculations", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Documents_CalculationId", table: "Documents", column: "CalculationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Documents_Calculations_CalculationId", table: "Documents");
            migrationBuilder.DropIndex(name: "IX_Documents_CalculationId", table: "Documents");
            migrationBuilder.DropColumn(name: "CalculationId", table: "Documents");
            migrationBuilder.DropTable(name: "Calculations");
        }
    }
}
