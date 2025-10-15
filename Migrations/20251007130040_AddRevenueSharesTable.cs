using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSharingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRevenueSharesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevenueShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevenueId = table.Column<int>(type: "int", nullable: false),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    ShareAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevenueShares_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevenueShares_Revenues_RevenueId",
                        column: x => x.RevenueId,
                        principalTable: "Revenues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevenueShares_PartnerId",
                table: "RevenueShares",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueShares_RevenueId",
                table: "RevenueShares",
                column: "RevenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevenueShares");
        }
    }
}
