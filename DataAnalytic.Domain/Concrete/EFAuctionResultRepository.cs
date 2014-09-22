using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Abstract;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Concrete
{
    public class EFAuctionResultRepository : IObjectRepository<AuctionResult>
    {
        private DataAnalytic.Domain.Concrete.EFDbContext context = new DataAnalytic.Domain.Concrete.EFDbContext();

        public IQueryable<AuctionResult> GetDataSet
        {
            get
            {
                return context.AuctionResults;
            }
        }

        public void SaveObject(AuctionResult entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateDate(string URL)
        {
            throw new NotImplementedException();
        }

        public int AddObject(AuctionResult entity)
        {
            if (entity.ID <= 0)
            {
                context.AuctionResults.Add(entity);
                context.SaveChanges();
                return entity.ID;
            }

            return -1;
        }

        public AuctionResult DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        public AuctionResult Find(int id)
        {
            throw new NotImplementedException();
        }

        /***********************************
        * customised method         * 
        * ***********************************/
        public void Append(AuctionResult entity)
        {
            AuctionResult dbEntity = context.AuctionResults.FirstOrDefault(p => p.Address == entity.Address && p.City == entity.City && p.Suburb == entity.Suburb);
            if (dbEntity == null) context.AuctionResults.Add(entity);
        }

        /***********************************
         * customised method         * 
         * ***********************************/
        public int SaveChange()
        {
            return context.SaveChanges();
        }

        public int AddOrUpdate(AuctionResult entity)
        {
            if (entity.ID <= 0)
            {
                context.AuctionResults.Add(entity);
                context.SaveChanges();
                return entity.ID;
            }
            else
            {
                AuctionResult dbEntity = context.AuctionResults.FirstOrDefault(p => p.ID == entity.ID);
                if (dbEntity != null)
                {
                    dbEntity.Suburb = entity.Suburb;
                    dbEntity.Address = entity.Address;
                    dbEntity.Type = entity.Type;
                    dbEntity.Price = entity.Price;
                    dbEntity.Result = entity.Result;
                    dbEntity.Agent = entity.Agent;
                    dbEntity.TransactionDate = entity.TransactionDate;
                    dbEntity.UpdatedDate = DateTime.Now;
                    context.SaveChanges();
                    return dbEntity.ID;
                }
                return -1;
            }
        
        }
    }
}
