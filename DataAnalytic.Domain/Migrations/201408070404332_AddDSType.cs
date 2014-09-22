namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDSType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataSources", "DataSourceType", c => c.String(nullable: false, maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataSources", "DataSourceType");
        }
    }
}
