namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCityIntoAuctionResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionResults", "City", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionResults", "City");
        }
    }
}
