using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Concrete
{
    public class EFSecurityRepository : IObjectRepository<Security>
    {
        private DataAnalytic.Domain.Entities.EFDbContext context = new DataAnalytic.Domain.Entities.EFDbContext();
        public IQueryable<Security> GetDataSet
        {
            get { return context.Securities; }
        }

        public void SaveObject(Security entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateDate(string URL)
        {
            throw new NotImplementedException();
        }

        public int AddObject(Security entity)
        {
            throw new NotImplementedException();
        }

        public Security DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        public Security Find(int id)
        {
            throw new NotImplementedException();
        }
    }
}
