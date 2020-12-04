using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    public interface ISingletonWrapper<TEntity>
    {
        TEntity Value { get; }
        TEntity Set(TEntity entity);
        TEntity Update(TEntity entity);
        void Remove();
    }
}
