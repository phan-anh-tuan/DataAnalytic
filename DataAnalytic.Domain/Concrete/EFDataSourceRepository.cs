using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Concrete
{
    public class EFDataSourceRepository : IObjectRepository<DataSource>
    {
        private DataAnalytic.Domain.Concrete.EFDbContext context = new DataAnalytic.Domain.Concrete.EFDbContext();
        public IQueryable<DataSource> GetDataSet
        {
            get
            {
                return context.DataSources;
            }
        }

        public int AddObject(DataSource datasource)
        {
            throw new NotImplementedException();
        }

        public void UpdateDate(string URL)
        {
            DataSource ds = context.DataSources.FirstOrDefault(ods => ods.URL == URL);
            if (ds != null)
            {
                ds.UpdatedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        public DataSource DeleteEntity(int dataSourceID)
        {
            throw new NotImplementedException();
        }

        public void SaveObject(DataSource entity)
        {
            throw new NotImplementedException();
        }


        public DataSource Find(int id)
        {
            throw new NotImplementedException();
        }
    }
}
