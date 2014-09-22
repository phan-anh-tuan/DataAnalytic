using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Entities;
using System.Data.Entity;

namespace DataAnalytic.Domain.Concrete
{
    public class EFDbContext: DbContext
    {
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<AuctionResult> AuctionResults { get; set; }
        //public DbSet<Country> Countries { get; set; }
        //public DbSet<Culture> Cultures { get; set; }
        //public DbSet<IndexDailyTransaction> IndexDailyTransactions { get; set; }
        //public DbSet<Indicy> Indicies { get; set; }
        //public DbSet<IndustryGroup> IndustryGroups { get; set; }
        //public DbSet<Language> Languages { get; set; }
        //public DbSet<Security> Securities { get; set; }
        //public DbSet<SecurityDailyTransaction> SecurityDailyTransactions { get; set; }
        //public DbSet<SecurityType> SecurityTypes { get; set; }
    }
}
