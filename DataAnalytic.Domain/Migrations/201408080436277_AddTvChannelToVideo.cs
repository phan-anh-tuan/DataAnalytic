namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTvChannelToVideo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Videos", "TvChannel", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "TvChannel");
        }
    }
}
