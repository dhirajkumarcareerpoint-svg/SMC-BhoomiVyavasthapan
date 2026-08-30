using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[Migration("20260827140000_AddDemandApplicationWorkflow")]
public partial class AddDemandApplicationWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DemandApplicationWorkflows",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                DemandApplicationId = table.Column<int>(type: "int", nullable: false),
                Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PaymentLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Utr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                PaymentScreenshotPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentScreenshotFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentScreenshotSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                PaymentSubmittedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                PaymentVerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PaymentVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CertificateFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CertificateFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CertificateGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Shera = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DemandApplicationWorkflows", x => x.Id);
                table.ForeignKey("FK_DemandApplicationWorkflows_DemandApplications_DemandApplicationId", x => x.DemandApplicationId, "DemandApplications", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_DemandApplicationWorkflows_DemandApplicationId", "DemandApplicationWorkflows", "DemandApplicationId", unique: true);
        migrationBuilder.CreateIndex("IX_DemandApplicationWorkflows_Stage_PaymentStatus", "DemandApplicationWorkflows", new[] { "Stage", "PaymentStatus" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("DemandApplicationWorkflows");
}