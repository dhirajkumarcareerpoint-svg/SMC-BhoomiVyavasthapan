using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
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

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [SmsTemplates] ON;
            INSERT INTO [SmsTemplates] ([Id], [EventType], [TemplateName], [MessageBody], [VariableMapping], [DltTemplateId], [Language], [Version], [IsActive], [ApprovalStatus], [CreatedBy], [CreatedAt], [IsDeleted], [Shera])
            VALUES
            (1, N'ApplicationSubmitted', N'Application Submitted', N'आपण {#alp#} साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक {#num#} आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा. - SMC Solapur - MAHGOV', N'["ServiceName","ApplicationNumber"]', N'1777178815669425110', N'mr', 1, 1, N'Approved', N'System', '2026-08-29T10:00:00Z', 0, NULL),
            (2, N'PaymentRequired', N'Payment Required', N'आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक {#num#} असून भरणा करावयाची रक्कम रु. {#num#}  आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा: {#urg#} {#uro#} SMC Solapur - MAHGOV', N'["ApplicationNumber","Amount","PaymentUrlPartOne","PaymentUrlPartTwo"]', N'1777178815696801613', N'mr', 1, 1, N'Approved', N'System', '2026-08-29T10:00:00Z', 0, NULL),
            (3, N'AssistantCommissionerNotification', N'Assistant Commissioner Notification', N'अर्ज क्रमांक {#num#} हा पुढिल कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV', N'["ApplicationNumber"]', N'1777178815700976811', N'mr', 1, 1, N'Approved', N'System', '2026-08-29T10:00:00Z', 0, NULL),
            (4, N'ApplicationApproved', N'Application Approved', N'आपल्या अर्जास मंजुरी देण्यात आली आहे. आपल्या अर्जाचा क्रमांक {#VAR}आहे. - SMC Solapur - MAHGOV', N'["ApplicationNumber"]', NULL, N'mr', 1, 1, N'PendingApproval', N'System', '2026-08-29T10:00:00Z', 0, NULL),
            (5, N'CertificateAvailable', N'Certificate Available', N'आपल्या {#VAR} अर्जाचे प्रमाणपत्र उपलब्ध झाले आहे. आपल्या अर्जाचा क्रमांक (#VAR)आहे. प्रमाणपत्र डाउनलोड करण्यासाठी खालील लिंकवर क्लिक करा: {#VAR} - SMC Solapur - MAHGOV', N'["ServiceName","ApplicationNumber","CertificateLink"]', NULL, N'mr', 1, 1, N'PendingApproval', N'System', '2026-08-29T10:00:00Z', 0, N'Needs confirmation against final DLT-approved template.');
            SET IDENTITY_INSERT [SmsTemplates] OFF;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SmsTemplates");
}
