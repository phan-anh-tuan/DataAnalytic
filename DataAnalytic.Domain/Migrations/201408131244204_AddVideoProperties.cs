namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVideoProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Videos", "Title", c => c.String());
            AddColumn("dbo.Videos", "Description", c => c.String());
            AddColumn("dbo.Videos", "Category", c => c.String());
            AddColumn("dbo.Videos", "Tags", c => c.String());
            AddColumn("dbo.Videos", "YouTubeId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Videos", "YouTubeId");
            DropColumn("dbo.Videos", "Tags");
            DropColumn("dbo.Videos", "Category");
            DropColumn("dbo.Videos", "Description");
            DropColumn("dbo.Videos", "Title");
        }
    }
}
