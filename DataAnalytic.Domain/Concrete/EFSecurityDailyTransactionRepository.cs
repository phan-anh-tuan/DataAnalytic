using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Concrete
{
    public class EFSecurityDailyTransactionRepository : IObjectRepository<SecurityDailyTransaction>
    {
        private DataAnalytic.Domain.Entities.EFDbContext context = new DataAnalytic.Domain.Entities.EFDbContext();
        public IQueryable<SecurityDailyTransaction> GetDataSet
        {
            get { return context.SecurityDailyTransactions; }
        }

        public void SaveObject(SecurityDailyTransaction entity)
        {
            throw new NotImplementedException();
        }

        public int AddObject(SecurityDailyTransaction entity)
        {
            if (entity.SecurityDailyTransactionID <= 0)
            {
                context.SecurityDailyTransactions.Add(entity);
                context.SaveChanges();
                return entity.SecurityDailyTransactionID;
            }
            return -1;
        }
        public void UpdateDate(string URL)
        {
            throw new NotImplementedException();
        }
        public SecurityDailyTransaction DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        public SecurityDailyTransaction Find(int id)
        {
            throw new NotImplementedException();
        }

        /***********************************
         * customised method         * 
         * ***********************************/
        public void Append(SecurityDailyTransaction entity)
        {
            context.SecurityDailyTransactions.Add(entity);
        }

        /***********************************
         * customised method         * 
         * ***********************************/
        public int SaveChange()
        {
            return context.SaveChanges();
        }
    }
}
