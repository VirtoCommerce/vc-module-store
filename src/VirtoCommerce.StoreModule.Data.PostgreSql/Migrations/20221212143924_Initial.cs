using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.StoreModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StoreState = table.Column<int>(type: "integer", nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Country = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Catalog = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreditCardSavePolicy = table.Column<int>(type: "integer", nullable: false),
                    SecureUrl = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    AdminEmail = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EmailName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AdminEmailName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DisplayOutOfStock = table.Column<bool>(type: "boolean", nullable: false),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ReturnsFulfillmentCenterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreCurrency",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCurrency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreCurrency_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreDynamicPropertyObjectValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ObjectType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ValueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShortTextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LongTextValue = table.Column<string>(type: "text", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,5)", nullable: true),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PropertyId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DictionaryItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreDynamicPropertyObjectValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreDynamicPropertyObjectValue_Store_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreFulfillmentCenter",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FulfillmentCenterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreFulfillmentCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreFulfillmentCenter_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreLanguage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreLanguage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreLanguage_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreSeoInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Keyword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImageAltDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    StoreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSeoInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreSeoInfo_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreTrustedGroup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreTrustedGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreTrustedGroup_Store_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreCurrency_StoreId",
                table: "StoreCurrency",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreDynamicPropertyObjectValue_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreDynamicPropertyObjectValue_ObjectType_ObjectId",
                table: "StoreDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_StoreFulfillmentCenter_StoreId",
                table: "StoreFulfillmentCenter",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreLanguage_StoreId",
                table: "StoreLanguage",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreSeoInfo_StoreId",
                table: "StoreSeoInfo",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreTrustedGroup_StoreId",
                table: "StoreTrustedGroup",
                column: "StoreId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreCurrency");

            migrationBuilder.DropTable(
                name: "StoreDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "StoreFulfillmentCenter");

            migrationBuilder.DropTable(
                name: "StoreLanguage");

            migrationBuilder.DropTable(
                name: "StoreSeoInfo");

            migrationBuilder.DropTable(
                name: "StoreTrustedGroup");

            migrationBuilder.DropTable(
                name: "Store");
        }
    }
}
