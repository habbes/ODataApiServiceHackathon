using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLib
{
    public abstract class ListDataStore : IDataStore
    {
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
            var type = this.GetType();
            var targetSetType = typeof(IDataSet<>).MakeGenericType(typeof(TEntity));

            var targetProp = type.GetProperties().FirstOrDefault(prop => targetSetType.IsAssignableFrom(prop.PropertyType));
            if (targetProp == null)
            {
                throw new Exception("Cannot find DataSet with that type");
            }

            var dataSet = targetProp.GetValue(this) as IDataSet<TEntity>;

            return dataSet;
        }

        private void InitDataSets()
        {
            var setOf = typeof(ListDataSet<>);
            var properties = this.GetType().GetProperties();

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
