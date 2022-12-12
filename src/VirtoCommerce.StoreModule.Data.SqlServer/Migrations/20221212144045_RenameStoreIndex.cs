using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StoreModule.Data.SqlServer.Migrations
{
    public partial class RenameStoreIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                newName: "IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                newName: "IX_ObjectType_ObjectId");
        }
    }
}
