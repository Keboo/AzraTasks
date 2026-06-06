using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzraTasks.Data.Migrations
{
    public partial class AllowDuplicateListNamesAcrossUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_FriendlyName",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CreatedByUserId_FriendlyName",
                table: "Rooms",
                columns: new[] { "CreatedByUserId", "FriendlyName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_CreatedByUserId_FriendlyName",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FriendlyName",
                table: "Rooms",
                column: "FriendlyName",
                unique: true);
        }
    }
}
