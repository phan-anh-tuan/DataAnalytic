namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuctionResult : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuctionResults",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TransactionDate = c.DateTime(nullable: false),
                        Suburb = c.String(nullable: false),
                        Address = c.String(nullable: false),
                        Type = c.String(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Result = c.String(nullable: false),
                        Agent = c.String(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AuctionResults");
        }
    }
}
