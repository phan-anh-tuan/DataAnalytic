using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAnalytic.Domain.Entities;

namespace DataAnalytic.Domain.Abstract
{
    public interface IObjectRepository<T>
    {
        IQueryable<T> GetDataSet { get; }
        void SaveObject(T entity);
        void UpdateDate(string URL);
        int AddObject(T entity);
        T DeleteEntity(int id);
        T Find(int id);
    }
}
