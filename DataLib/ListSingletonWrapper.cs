using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    public class ListSingletonWrapper<TEntity> : ISingletonWrapper<TEntity>
    {
        IDictionary<string, object> values;
        string name;

        public ListSingletonWrapper(string name, IDictionary<string, object> values)
        {
            this.name = name;
            this.values = values;
        }
        public TEntity Value
        {
            get
            {
                if (values.TryGetValue(name, out object result))
                {
                    return (TEntity)result;
                }

                return default;
            }
        }

        public void Remove()
        {
            values.Remove(name);
        }

        public TEntity Set(TEntity entity)
        {
            values[name] = entity;
            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            values[name] = entity;
            return entity;
        }
    }
}
