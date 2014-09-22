namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendDataSourceURLLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DataSources", "URL", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DataSources", "URL", c => c.String(nullable: false, maxLength: 50));
        }
    }
}
