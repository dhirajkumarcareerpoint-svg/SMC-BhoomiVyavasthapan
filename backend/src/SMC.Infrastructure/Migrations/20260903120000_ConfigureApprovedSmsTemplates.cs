using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260903120000_ConfigureApprovedSmsTemplates")]
public partial class ConfigureApprovedSmsTemplates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE [SmsTemplates] SET [DltTemplateId] = N'1777178815669425110', [MessageBody] = N'आपण {#alp#} साठी अर्ज सादर केला आहे. आपल्या अर्जाचा क्रमांक {#num#} आहे. कृपया भविष्यातील कार्यवाहीसाठी अर्ज क्रमांक जतन करून ठेवा. - SMC Solapur - MAHGOV', [VariableMapping] = N'["ServiceName","ApplicationNumber"]', [ApprovalStatus] = N'Approved' WHERE [EventType] = N'ApplicationSubmitted';
            UPDATE [SmsTemplates] SET [DltTemplateId] = N'1777178815696801613', [MessageBody] = N'आपल्या अर्जाची प्राथमिक तपासणी पूर्ण झाली आहे. आपल्या अर्जाचा क्रमांक {#num#} असून भरणा करावयाची रक्कम रु. {#num#}  आहे. शुल्क भरण्यासाठी खालील लिंकवर क्लिक करा: {#urg#} {#uro#} SMC Solapur - MAHGOV', [VariableMapping] = N'["ApplicationNumber","Amount","PaymentUrlPartOne","PaymentUrlPartTwo"]', [ApprovalStatus] = N'Approved', [Shera] = NULL WHERE [EventType] = N'PaymentRequired';
            UPDATE [SmsTemplates] SET [DltTemplateId] = N'1777178815700976811', [MessageBody] = N'अर्ज क्रमांक {#num#} हा पुढिल कार्यवाहीसाठी प्राप्त झाला आहे. कृपया पुढील आवश्यक कार्यवाही करावी. - SMC Solapur - MAHGOV', [VariableMapping] = N'["ApplicationNumber"]', [ApprovalStatus] = N'Approved' WHERE [EventType] = N'AssistantCommissionerNotification';
            """);
        migrationBuilder.DropIndex(name: "IX_SmsEvents_ApplicationNumber_EventType_CreatedAt", table: "SmsEvents");
        migrationBuilder.CreateIndex(name: "IX_SmsEvents_ApplicationNumber_EventType", table: "SmsEvents", columns: new[] { "ApplicationNumber", "EventType" }, unique: true, filter: "[ApplicationNumber] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_SmsEvents_ApplicationNumber_EventType", table: "SmsEvents");
        migrationBuilder.CreateIndex(name: "IX_SmsEvents_ApplicationNumber_EventType_CreatedAt", table: "SmsEvents", columns: new[] { "ApplicationNumber", "EventType", "CreatedAt" });
    }
}
