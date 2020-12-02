using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public abstract class ListDataStore : IDataStore
    {
        ConcurrentDictionary<string, object> singletons = new ConcurrentDictionary<string, object>();
        public ListDataStore()
        {
            InitDataSets();
        }
        public virtual void SaveChanges()
        {
        }

        public virtual Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        public IDataSet<TEntity> Set<TEntity>()
        {
            var type = GetType();
            var targetSetType = typeof(IDataSet<>).MakeGenericType(typeof(TEntity));

            var targetProp = type.GetProperties().FirstOrDefault(prop => targetSetType.IsAssignableFrom(prop.PropertyType));
            if (targetProp == null)
            {
                throw new Exception("Cannot find DataSet with that type");
            }

            var dataSet = targetProp.GetValue(this) as IDataSet<TEntity>;

            return dataSet;
        }

        public ISingletonWrapper<TEntity> Singleton<TEntity>(string name)
        {
            var wrapper = new ValuesLookupSingletonWrapper<TEntity>(name, singletons);
            return wrapper;
        }

        public TEntity GetSingleton<TEntity>(string name)
        {
            if (singletons.TryGetValue(name, out object result))
            {
                return (TEntity)result;
            }

            return default;
        }

        public void RemoveSingleton<TEntity>(string name)
        {
            singletons.TryRemove(name, out object result);
        }

        private void InitDataSets()
        {
            var setOf = typeof(ListDataSet<>);
            var properties = GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.PropertyType.IsGenericType)
                {
                    continue;
                }

                var entityType = prop.PropertyType.GetGenericArguments()[0];
                var setType = setOf.MakeGenericType(entityType);

                if (!setType.IsAssignableFrom(prop.PropertyType))
                {
                    continue;
                }

                var newSet = Activator.CreateInstance(setType);
                prop.SetValue(this, newSet);
            }
        }
    }
}
