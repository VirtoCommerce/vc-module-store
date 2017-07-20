namespace VirtoCommerce.StoreModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StoreFulfillmentCenters : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StoreFulfillmentCenter",
                c => new
                    {
                        FulfillmentCenterId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Type = c.String(),
                        StoreId = c.String(nullable: false, maxLength: 128),
                        Id = c.String(),
                    })
                .PrimaryKey(t => t.FulfillmentCenterId)
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
