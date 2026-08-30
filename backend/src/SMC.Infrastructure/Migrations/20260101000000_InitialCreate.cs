using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260101000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Shera = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PropertyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ward = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AreaSqFt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualDemand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurveyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TpNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentOccupant = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Shera = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Properties", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Leases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    LesseeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LesseeMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LesseeAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeedNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SecurityDeposit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_Leases", x => x.Id);
                    table.ForeignKey("FK_Leases_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecoveryCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    LeaseId = table.Column<int>(type: "int", nullable: true),
                    MonthsOverdue = table.Column<int>(type: "int", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NoticeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NoticeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecoveredAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecoveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SealDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReAuctionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_RecoveryCases", x => x.Id);
                    table.ForeignKey("FK_RecoveryCases_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                    table.ForeignKey("FK_RecoveryCases_Leases_LeaseId", x => x.LeaseId, "Leases", "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchemeApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SchemeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApplicantMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaivedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
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
                    table.PrimaryKey("PK_SchemeApplications", x => x.Id);
                    table.ForeignKey("FK_SchemeApplications_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                });

            migrationBuilder.CreateTable(
                name: "AllocationProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NoticeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastDateToApply = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuctionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReserveAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HighestBidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HighestBidderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HighestBidderMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
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
                    table.PrimaryKey("PK_AllocationProcesses", x => x.Id);
                    table.ForeignKey("FK_AllocationProcesses_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: true),
                    LeaseId = table.Column<int>(type: "int", nullable: true),
                    RecoveryCaseId = table.Column<int>(type: "int", nullable: true),
                    SchemeApplicationId = table.Column<int>(type: "int", nullable: true),
                    AllocationProcessId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey("FK_Documents_Properties_PropertyId", x => x.PropertyId, "Properties", "Id");
                    table.ForeignKey("FK_Documents_Leases_LeaseId", x => x.LeaseId, "Leases", "Id");
                    table.ForeignKey("FK_Documents_RecoveryCases_RecoveryCaseId", x => x.RecoveryCaseId, "RecoveryCases", "Id");
                    table.ForeignKey("FK_Documents_SchemeApplications_SchemeApplicationId", x => x.SchemeApplicationId, "SchemeApplications", "Id");
                    table.ForeignKey("FK_Documents_AllocationProcesses_AllocationProcessId", x => x.AllocationProcessId, "AllocationProcesses", "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey("FK_AuditLogs_Users_UserId", x => x.UserId, "Users", "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_Users_Username", table: "Users", column: "Username", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Properties_PropertyCode", table: "Properties", column: "PropertyCode");
            migrationBuilder.CreateIndex(name: "IX_Properties_Category", table: "Properties", column: "Category");
            migrationBuilder.CreateIndex(name: "IX_Properties_Status", table: "Properties", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Properties_Ward", table: "Properties", column: "Ward");
            migrationBuilder.CreateIndex(name: "IX_Leases_PropertyId", table: "Leases", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_Leases_DeedNumber", table: "Leases", column: "DeedNumber");
            migrationBuilder.CreateIndex(name: "IX_Leases_Status", table: "Leases", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_RecoveryCases_PropertyId", table: "RecoveryCases", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_RecoveryCases_LeaseId", table: "RecoveryCases", column: "LeaseId");
            migrationBuilder.CreateIndex(name: "IX_RecoveryCases_Stage", table: "RecoveryCases", column: "Stage");
            migrationBuilder.CreateIndex(name: "IX_SchemeApplications_PropertyId", table: "SchemeApplications", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_SchemeApplications_SchemeType", table: "SchemeApplications", column: "SchemeType");
            migrationBuilder.CreateIndex(name: "IX_AllocationProcesses_PropertyId", table: "AllocationProcesses", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_AllocationProcesses_Method", table: "AllocationProcesses", column: "Method");
            migrationBuilder.CreateIndex(name: "IX_AllocationProcesses_Status", table: "AllocationProcesses", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Documents_EntityType_EntityId", table: "Documents", columns: new[] { "EntityType", "EntityId" });
            migrationBuilder.CreateIndex(name: "IX_Documents_PropertyId", table: "Documents", column: "PropertyId");
            migrationBuilder.CreateIndex(name: "IX_Documents_LeaseId", table: "Documents", column: "LeaseId");
            migrationBuilder.CreateIndex(name: "IX_Documents_RecoveryCaseId", table: "Documents", column: "RecoveryCaseId");
            migrationBuilder.CreateIndex(name: "IX_Documents_SchemeApplicationId", table: "Documents", column: "SchemeApplicationId");
            migrationBuilder.CreateIndex(name: "IX_Documents_AllocationProcessId", table: "Documents", column: "AllocationProcessId");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_UserId", table: "AuditLogs", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_EntityName_EntityId", table: "AuditLogs", columns: new[] { "EntityName", "EntityId" });
            migrationBuilder.CreateIndex(name: "IX_AuditLogs_Timestamp", table: "AuditLogs", column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "Documents");
            migrationBuilder.DropTable(name: "AllocationProcesses");
            migrationBuilder.DropTable(name: "SchemeApplications");
            migrationBuilder.DropTable(name: "RecoveryCases");
            migrationBuilder.DropTable(name: "Leases");
            migrationBuilder.DropTable(name: "Properties");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
