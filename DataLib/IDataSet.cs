using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    public interface IDataSet<TEntity>: IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable
    {
        TEntity Add(TEntity entity);
        void Remove(TEntity entity);
        void Update(TEntity entity);
        TEntity FindByKey(params object[] keysValues);
    }
}
