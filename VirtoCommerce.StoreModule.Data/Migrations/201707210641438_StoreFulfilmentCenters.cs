namespace VirtoCommerce.StoreModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreFulfilmentCenters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StoreFulfillmentCenter",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Type = c.String(nullable: false, maxLength: 32),
                        FulfillmentCenterId = c.String(nullable: false, maxLength: 128),
                        StoreId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Store", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StoreFulfillmentCenter", "StoreId", "dbo.Store");
            DropIndex("dbo.StoreFulfillmentCenter", new[] { "StoreId" });
            DropTable("dbo.StoreFulfillmentCenter");
        }
    }
}
