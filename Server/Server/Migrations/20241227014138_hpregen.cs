using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class hpregen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnchartedPointRegen",
                table: "Player",
                newName: "UpRegen");

            migrationBuilder.RenameColumn(
                name: "UnchartedPoint",
                table: "Player",
                newName: "HpRegen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpRegen",
                table: "Player",
                newName: "UnchartedPointRegen");

            migrationBuilder.RenameColumn(
                name: "HpRegen",
                table: "Player",
                newName: "UnchartedPoint");
        }
    }
}
