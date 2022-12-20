using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.StoreModule.Data.SqlServer.Migrations
{
    public partial class AddEmailNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminEmailName",
                table: "Store",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailName",
                table: "Store",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminEmailName",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "EmailName",
                table: "Store");
        }
    }
}
