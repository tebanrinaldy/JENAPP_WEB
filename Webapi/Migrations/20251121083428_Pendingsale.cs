using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webapi.Migrations
{
    /// <inheritdoc />
    public partial class Pendingsale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PendingSaleDetails_PendingSales_PendingSaleId",
                table: "PendingSaleDetails");

            migrationBuilder.AlterColumn<int>(
                name: "PendingSaleId",
                table: "PendingSaleDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PendingSaleDetails_PendingSales_PendingSaleId",
                table: "PendingSaleDetails",
                column: "PendingSaleId",
                principalTable: "PendingSales",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PendingSaleDetails_PendingSales_PendingSaleId",
                table: "PendingSaleDetails");

            migrationBuilder.AlterColumn<int>(
                name: "PendingSaleId",
                table: "PendingSaleDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PendingSaleDetails_PendingSales_PendingSaleId",
                table: "PendingSaleDetails",
                column: "PendingSaleId",
                principalTable: "PendingSales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
