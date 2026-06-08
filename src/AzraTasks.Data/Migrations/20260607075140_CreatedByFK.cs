using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzraTasks.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatedByFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_AspNetUsers_CreatedById",
                table: "TodoLists");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "TodoLists",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_AspNetUsers_CreatedById",
                table: "TodoLists",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_AspNetUsers_CreatedById",
                table: "TodoLists");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "TodoLists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_AspNetUsers_CreatedById",
                table: "TodoLists",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
