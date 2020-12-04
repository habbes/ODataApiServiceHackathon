using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public interface IDataStore
    {
        IDataSet<TEntity> Set<TEntity>();
        ISingletonWrapper<TEntity> Singleton<TEntity>(string name);
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
