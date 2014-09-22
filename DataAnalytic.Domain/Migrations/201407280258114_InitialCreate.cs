namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataSources",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        URL = c.String(nullable: false, maxLength: 50),
                        FileFormat = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DataSources");
        }
    }
}
