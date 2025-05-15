using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StoreModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOuterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Store_OuterId",
                table: "Store",
                column: "OuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Store_OuterId",
                table: "Store");
        }
    }
}
