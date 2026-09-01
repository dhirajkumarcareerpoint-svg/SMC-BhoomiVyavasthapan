using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDemandDocumentRequestTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "VerificationStatus", table: "DemandApplicationDocuments", type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Unchecked");
            migrationBuilder.AddColumn<string>(name: "RequestRemark", table: "DemandApplicationDocuments", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "RequestedAt", table: "DemandApplicationDocuments", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "RequestedBy", table: "DemandApplicationDocuments", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "RespondedAt", table: "DemandApplicationDocuments", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "RequestTokenHash", table: "DemandApplicationDocuments", type: "nvarchar(64)", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "RequestTokenCreatedAt", table: "DemandApplicationDocuments", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "RequestTokenConsumedAt", table: "DemandApplicationDocuments", type: "datetime2", nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "VerificationStatus", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestRemark", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestedAt", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestedBy", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RespondedAt", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestTokenHash", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestTokenCreatedAt", table: "DemandApplicationDocuments"); migrationBuilder.DropColumn(name: "RequestTokenConsumedAt", table: "DemandApplicationDocuments");
        }
    }
}
