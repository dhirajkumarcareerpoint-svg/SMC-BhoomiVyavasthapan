using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SMC.Infrastructure.Persistence;

#nullable disable

namespace SMC.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260828100000_AddDemandOfficerRoles")]
public partial class AddDemandOfficerRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Users SET Role = 'JE' WHERE Username = 'officer1' AND Role = 'Officer';");
        migrationBuilder.Sql("UPDATE Users SET Role = 'OS' WHERE Username = 'officer2' AND Role = 'Officer';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Users SET Role = 'Officer' WHERE Username IN ('officer1', 'officer2') AND Role IN ('JE', 'OS');");
    }
}
