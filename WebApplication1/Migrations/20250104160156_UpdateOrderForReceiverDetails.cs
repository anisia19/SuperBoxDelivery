using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderForReceiverDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuperBoxes_Orders_OrderId",
                table: "SuperBoxes");

            migrationBuilder.DropIndex(
                name: "IX_SuperBoxes_OrderId",
                table: "SuperBoxes");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverSuperBoxId",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverUserId",
                table: "Orders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReceiverSuperBoxId",
                table: "Orders",
                column: "ReceiverSuperBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReceiverUserId",
                table: "Orders",
                column: "ReceiverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_SuperBoxes_ReceiverSuperBoxId",
                table: "Orders",
                column: "ReceiverSuperBoxId",
                principalTable: "SuperBoxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_ReceiverUserId",
                table: "Orders",
                column: "ReceiverUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_SuperBoxes_ReceiverSuperBoxId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ReceiverUserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReceiverSuperBoxId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReceiverUserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiverSuperBoxId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceiverUserId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_SuperBoxes_OrderId",
                table: "SuperBoxes",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuperBoxes_Orders_OrderId",
                table: "SuperBoxes",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");
        }
    }
}
