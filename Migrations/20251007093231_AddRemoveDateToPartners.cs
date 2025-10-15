using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSharingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRemoveDateToPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveDate",
                table: "Partners",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemoveDate",
                table: "Partners");
        }
    }
}
