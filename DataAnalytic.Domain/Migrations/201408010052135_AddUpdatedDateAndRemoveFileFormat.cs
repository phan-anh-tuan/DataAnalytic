namespace DataAnalytic.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUpdatedDateAndRemoveFileFormat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataSources", "UpdatedDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.DataSources", "FileFormat");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataSources", "FileFormat", c => c.String());
            DropColumn("dbo.DataSources", "UpdatedDate");
        }
    }
}
