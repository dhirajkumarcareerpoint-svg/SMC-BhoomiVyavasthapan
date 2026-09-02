using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260902100000_AddRazorpayPaymentFields")]
public partial class AddRazorpayPaymentFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RazorpayOrderId",
            table: "DemandApplicationWorkflows",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RazorpayPaymentId",
            table: "DemandApplicationWorkflows",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RazorpaySignature",
            table: "DemandApplicationWorkflows",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "RazorpayOrderId", table: "DemandApplicationWorkflows");
        migrationBuilder.DropColumn(name: "RazorpayPaymentId", table: "DemandApplicationWorkflows");
        migrationBuilder.DropColumn(name: "RazorpaySignature", table: "DemandApplicationWorkflows");
    }
}
