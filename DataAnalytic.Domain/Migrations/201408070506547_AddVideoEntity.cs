namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVideoEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Videos",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        URL = c.String(nullable: false, maxLength: 256),
                        IsAvailable = c.Boolean(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Videos");
        }
    }
}
