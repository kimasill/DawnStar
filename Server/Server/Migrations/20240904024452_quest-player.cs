using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class questplayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Player_PlayerDbId",
                table: "Quest");

            migrationBuilder.RenameColumn(
                name: "PlayerDbId",
                table: "Quest",
                newName: "OwnerDbId");

            migrationBuilder.RenameIndex(
                name: "IX_Quest_PlayerDbId",
                table: "Quest",
                newName: "IX_Quest_OwnerDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Player_OwnerDbId",
                table: "Quest",
                column: "OwnerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Player_OwnerDbId",
                table: "Quest");

            migrationBuilder.RenameColumn(
                name: "OwnerDbId",
                table: "Quest",
                newName: "PlayerDbId");

            migrationBuilder.RenameIndex(
                name: "IX_Quest_OwnerDbId",
                table: "Quest",
                newName: "IX_Quest_PlayerDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Player_PlayerDbId",
                table: "Quest",
                column: "PlayerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId");
        }
    }
}
