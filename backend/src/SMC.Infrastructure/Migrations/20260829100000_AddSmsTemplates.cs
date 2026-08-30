using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[Migration("20260829100000_AddSmsTemplates")]
public partial class AddSmsTemplates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SmsTemplates",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EventType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                TemplateName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                VariableMapping = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                DltTemplateId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                SenderId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                ApprovalStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                SampleMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Shera = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_SmsTemplates", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_SmsTemplates_EventType_IsActive", table: "SmsTemplates", columns: new[] { "EventType", "IsActive" });

        migrationBuilder.InsertData("SmsTemplates", new[] { "Id", "EventType", "TemplateName", "MessageBody", "VariableMapping", "Language", "Version", "IsActive", "ApprovalStatus", "CreatedBy", "CreatedAt", "IsDeleted", "Shera" }, new object[,]
        {
            { 1, "ApplicationSubmitted", "Application Submitted", "आपण  + {#VAR}  साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक  + {#VAR} +  आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा.\n- SMC Solapur - MAHGOV", "[\"ServiceName\",\"ApplicationNumber\"]", "mr", 1, true, "PendingApproval", "System", new DateTime(2026, 8, 29, 10, 0, 0, DateTimeKind.Utc), false, null },
            { 2, "PaymentRequired", "Payment Required", "आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक {#VAR} असून भरणा करावयाची रक्कम रु. {#VAR} आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा:\n{#VAR}{#VAR} SMC Solapur - MAHGOV", "[\"ApplicationNumber\",\"Amount\",\"PaymentLink\",\"UnmappedPlaceholder\"]", "mr", 1, true, "PendingApproval", "System", new DateTime(2026, 8, 29, 10, 0, 0, DateTimeKind.Utc), false, "Needs confirmation against final DLT-approved template." },
            { 3, "AssistantCommissionerNotification", "Assistant Commissioner Notification", "अर्ज क्रमांक {#VAR} हा पुढील कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV", "[\"ApplicationNumber\"]", "mr", 1, true, "PendingApproval", "System", new DateTime(2026, 8, 29, 10, 0, 0, DateTimeKind.Utc), false, null },
            { 4, "ApplicationApproved", "Application Approved", "आपल्या अर्जास मंजुरी देण्यात आली आहे. आपल्या अर्जाचा क्रमांक {#VAR}आहे. - SMC Solapur - MAHGOV", "[\"ApplicationNumber\"]", "mr", 1, true, "PendingApproval", "System", new DateTime(2026, 8, 29, 10, 0, 0, DateTimeKind.Utc), false, null },
            { 5, "CertificateAvailable", "Certificate Available", "आपल्या {#VAR} अर्जाचे प्रमाणपत्र उपलब्ध झाले आहे. आपल्या अर्जाचा क्रमांक (#VAR)आहे. प्रमाणपत्र डाउनलोड करण्यासाठी खालील लिंकवर क्लिक करा: {#VAR} - SMC Solapur - MAHGOV", "[\"ServiceName\",\"ApplicationNumber\",\"CertificateLink\"]", "mr", 1, true, "PendingApproval", "System", new DateTime(2026, 8, 29, 10, 0, 0, DateTimeKind.Utc), false, "Needs confirmation against final DLT-approved template." }
        });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SmsTemplates");
}
