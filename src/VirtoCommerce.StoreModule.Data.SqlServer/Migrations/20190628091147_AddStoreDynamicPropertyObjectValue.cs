using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.StoreModule.Data.SqlServer.Migrations
{
    public partial class AddStoreDynamicPropertyObjectValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreDynamicPropertyObjectValue",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ObjectType = table.Column<string>(maxLength: 256, nullable: true),
                    ObjectId = table.Column<string>(maxLength: 128, nullable: true),
                    Locale = table.Column<string>(maxLength: 64, nullable: true),
                    ValueType = table.Column<string>(maxLength: 64, nullable: false),
                    ShortTextValue = table.Column<string>(maxLength: 512, nullable: true),
                    LongTextValue = table.Column<string>(nullable: true),
                    DecimalValue = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    IntegerValue = table.Column<int>(nullable: true),
                    BooleanValue = table.Column<bool>(nullable: true),
                    DateTimeValue = table.Column<DateTime>(nullable: true),
                    PropertyId = table.Column<string>(maxLength: 128, nullable: true),
                    DictionaryItemId = table.Column<string>(maxLength: 128, nullable: true),
                    PropertyName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreDynamicPropertyObjectValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreDynamicPropertyObjectValue_Store_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreDynamicPropertyObjectValue_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectType_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ObjectId" });



            migrationBuilder.Sql(@"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = '__MigrationHistory'))
                    BEGIN
                        INSERT INTO [StoreDynamicPropertyObjectValue] ([Id],[CreatedDate],[ModifiedDate],[CreatedBy],[ModifiedBy],[ObjectType],[ObjectId],[Locale],[ValueType],[ShortTextValue],[LongTextValue],[DecimalValue],[IntegerValue],[BooleanValue],[DateTimeValue],[PropertyId],[DictionaryItemId], [PropertyName])
                        SELECT OV.[Id],OV.[CreatedDate],OV.[ModifiedDate],OV.[CreatedBy],OV.[ModifiedBy],OV.[ObjectType],OV.[ObjectId],[Locale],OV.[ValueType],[ShortTextValue],[LongTextValue],[DecimalValue],[IntegerValue],[BooleanValue],[DateTimeValue],[PropertyId],[DictionaryItemId], PDP.[Name]
                        FROM [PlatformDynamicPropertyObjectValue] OV
						INNER JOIN [PlatformDynamicProperty] PDP ON PDP.Id = OV.PropertyId
                        WHERE OV.ObjectType = 'VirtoCommerce.StoreModule.Core.Model.Store'
                    END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreDynamicPropertyObjectValue");
        }
    }
}
