namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNumberOfBedRoomToAuctionResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionResults", "NoOfBedroom", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionResults", "NoOfBedroom");
        }
    }
}
