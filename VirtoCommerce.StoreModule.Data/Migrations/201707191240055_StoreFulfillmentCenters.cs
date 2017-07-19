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
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Type = c.Int(nullable: false),
                        StoreId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Store", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
            DropColumn("dbo.Store", "OuterStoreId");
            DropColumn("dbo.Store", "StoreType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Store", "StoreType", c => c.String(maxLength: 128));
            AddColumn("dbo.Store", "OuterStoreId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.StoreFulfillmentCenter", "StoreId", "dbo.Store");
            DropIndex("dbo.StoreFulfillmentCenter", new[] { "StoreId" });
            DropTable("dbo.StoreFulfillmentCenter");
        }
    }
}
