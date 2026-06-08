using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzraTasks.Data.Migrations
{
    /// <inheritdoc />
    public partial class TrackingBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastModifiedDate",
                table: "TodoLists",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "TodoLists");
        }
    }
}
