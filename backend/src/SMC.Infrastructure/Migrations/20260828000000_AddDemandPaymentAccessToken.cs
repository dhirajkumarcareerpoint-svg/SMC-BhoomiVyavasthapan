using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
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
