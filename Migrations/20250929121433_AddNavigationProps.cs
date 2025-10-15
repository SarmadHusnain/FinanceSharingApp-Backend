using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceSharingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Partners_PaidByPartnerId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Revenues_Partners_ReceivedByPartnerId",
                table: "Revenues");

            migrationBuilder.RenameColumn(
                name: "ReceivedByPartnerId",
                table: "Revenues",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "Revenues",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_Revenues_ReceivedByPartnerId",
                table: "Revenues",
                newName: "IX_Revenues_PartnerId");

            migrationBuilder.RenameColumn(
                name: "PaidByPartnerId",
                table: "Expenses",
                newName: "PartnerId");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "Expenses",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_PaidByPartnerId",
                table: "Expenses",
                newName: "IX_Expenses_PartnerId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Revenues",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Partners_PartnerId",
                table: "Expenses",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Revenues_Partners_PartnerId",
                table: "Revenues",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Partners_PartnerId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Revenues_Partners_PartnerId",
                table: "Revenues");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "Revenues",
                newName: "ReceivedByPartnerId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Revenues",
                newName: "OccurredAt");

            migrationBuilder.RenameIndex(
                name: "IX_Revenues_PartnerId",
                table: "Revenues",
                newName: "IX_Revenues_ReceivedByPartnerId");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                table: "Expenses",
                newName: "PaidByPartnerId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Expenses",
                newName: "OccurredAt");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_PartnerId",
                table: "Expenses",
                newName: "IX_Expenses_PaidByPartnerId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Revenues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Partners_PaidByPartnerId",
                table: "Expenses",
                column: "PaidByPartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Revenues_Partners_ReceivedByPartnerId",
                table: "Revenues",
                column: "ReceivedByPartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
