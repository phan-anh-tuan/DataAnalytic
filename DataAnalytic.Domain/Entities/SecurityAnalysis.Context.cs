﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAnalytic.Domain.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EFDbContext : DbContext
    {
        public EFDbContext()
            : base("name=EFDbFirstContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Culture> Cultures { get; set; }
        public DbSet<IndexDailyTransaction> IndexDailyTransactions { get; set; }
        public DbSet<Indicy> Indicies { get; set; }
        public DbSet<IndustryGroup> IndustryGroups { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Security> Securities { get; set; }
        public DbSet<SecurityDailyTransaction> SecurityDailyTransactions { get; set; }
        public DbSet<SecurityType> SecurityTypes { get; set; }
    }
}
