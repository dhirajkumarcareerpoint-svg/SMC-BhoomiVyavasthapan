using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[Migration("20260828000000_AddDemandPaymentAccessToken")]
public partial class AddDemandPaymentAccessToken : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "PaymentAccessToken", table: "DemandApplicationWorkflows", type: "nvarchar(128)", maxLength: 128, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PaymentAccessToken", table: "DemandApplicationWorkflows");
    }
}
