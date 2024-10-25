using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class shopitemdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Shop_ShopDbId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_ShopDbId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "ShopDbId",
                table: "Item");

            migrationBuilder.CreateTable(
                name: "ShopItem",
                columns: table => new
                {
                    ShopItemDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopDbId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopItem", x => x.ShopItemDbId);
                    table.ForeignKey(
                        name: "FK_ShopItem_Shop_ShopDbId",
                        column: x => x.ShopDbId,
                        principalTable: "Shop",
                        principalColumn: "ShopDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopItem_ShopDbId",
                table: "ShopItem",
                column: "ShopDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopItem");

            migrationBuilder.AddColumn<int>(
                name: "ShopDbId",
                table: "Item",
                type: "int",
                nullable: true);

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
    }
}
