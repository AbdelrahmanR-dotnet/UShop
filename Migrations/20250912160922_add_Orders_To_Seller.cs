using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UShop.Migrations
{
    /// <inheritdoc />
    public partial class add_Orders_To_Seller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_Orders_OrderId",
                table: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Sellers_OrderId",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Sellers");

            migrationBuilder.CreateTable(
                name: "OrderSeller",
                columns: table => new
                {
                    OrdersId = table.Column<int>(type: "int", nullable: false),
                    SellersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSeller", x => new { x.OrdersId, x.SellersId });
                    table.ForeignKey(
                        name: "FK_OrderSeller_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderSeller_Sellers_SellersId",
                        column: x => x.SellersId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSeller_SellersId",
                table: "OrderSeller",
                column: "SellersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSeller");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Sellers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_OrderId",
                table: "Sellers",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_Orders_OrderId",
                table: "Sellers",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
