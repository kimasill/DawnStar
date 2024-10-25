using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class shop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShopDbId",
                table: "Item",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Shop",
                columns: table => new
                {
                    ShopDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    ShopName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scene = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerDbId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shop", x => x.ShopDbId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_ShopDbId",
                table: "Item",
                column: "ShopDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Shop_ShopDbId",
                table: "Item",
                column: "ShopDbId",
                principalTable: "Shop",
                principalColumn: "ShopDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Shop_ShopDbId",
                table: "Item");

            migrationBuilder.DropTable(
                name: "Shop");

            migrationBuilder.DropIndex(
                name: "IX_Item_ShopDbId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "ShopDbId",
                table: "Item");
        }
    }
}
