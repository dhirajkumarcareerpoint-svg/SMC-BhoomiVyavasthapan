using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[Migration("20260829090000_AddSmsEvents")]
public partial class AddSmsEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SmsEvents",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                RecipientMobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                ApplicationNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                TemplateReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                ProviderMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                RetryCount = table.Column<int>(type: "int", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Shera = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SmsEvents", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_SmsEvents_ApplicationNumber_EventType_CreatedAt", table: "SmsEvents", columns: new[] { "ApplicationNumber", "EventType", "CreatedAt" });
        migrationBuilder.CreateIndex(name: "IX_SmsEvents_Status_CreatedAt", table: "SmsEvents", columns: new[] { "Status", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SmsEvents");
}
