using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.StoreModule.Data.Migrations
{
    public partial class UpdateLoginOnBehalf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE [AspNetRoleClaims] SET  [ClaimValue] = 'platform:security:loginOnBehalf' WHERE [ClaimValue] = 'store:loginOnBehalf'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE [AspNetRoleClaims] SET  [ClaimValue] = 'store:loginOnBehalf' WHERE [ClaimValue] = 'platform:security:loginOnBehalf'");
        }
    }
}
