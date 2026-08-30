using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DemandPaymentAccessTokenFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 20260828000000_AddDemandPaymentAccessToken already creates this column.
            // Keep this migration ID as a no-op so existing migration histories remain
            // valid without attempting the same ADD COLUMN a second time.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: the preceding migration owns this column.
        }
    }
}
