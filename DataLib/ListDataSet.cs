using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataLib
{
    public class ListDataSet<TEntity> : IDataSet<TEntity>
    {
        List<TEntity> data = new List<TEntity>();

        Type IQueryable.ElementType => typeof(TEntity);

        Expression IQueryable.Expression => data.AsQueryable().Expression;

        IQueryProvider IQueryable.Provider => data.AsQueryable().Provider;

        public TEntity Add(TEntity entity)
        {
            var keys = GetKeysValues(entity);

            foreach (var key in keys)
            {
                if (key == null)
                {
                    throw new Exception("Key cannot be null");
                }
            }

            var duplicate = FindByKey(keys);

            if (duplicate != null)
            {
                throw new Exception("Duplicate key");
            }

            InitializeListProperties(entity);
            data.Add(entity);

            return entity;
        }

        public TEntity FindByKey(params object[] keysValues)
        {
            return data.FirstOrDefault(entity => EntityMatchesKeys(entity, keysValues));
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public void Remove(TEntity entity)
        {
            data.Remove(entity);
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }


        private IEnumerable<PropertyInfo> GetKeyProperties()
        {
            var type = typeof(TEntity);
            return type.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any());
        }

        private bool EntityMatchesKeys(TEntity entity, params object[] keysValues)
        {
            var keyFields = GetKeyProperties();
            var kvps = keyFields.Zip(keysValues, (k, v) => new KeyValuePair<PropertyInfo, object>(k, v));
            foreach (var kvp in kvps)
            {
                if (!kvp.Key.GetValue(entity).Equals(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private object[] GetKeysValues(TEntity entity)
        {
            var values = new List<object>();
            var keyFields = GetKeyProperties();
            foreach (var field in keyFields)
            {
                values.Add(field.GetValue(entity));
            }

            return values.ToArray();
        }

        private void InitializeListProperties(TEntity entity)
        {
            var properties = typeof(TEntity).GetProperties();
            var listOfType = typeof(List<>);

            foreach (var prop in properties)
            {
                if (!prop.PropertyType.IsGenericType) continue;
                var elementType = prop.PropertyType.GetGenericArguments()[0];
                var listType = listOfType.MakeGenericType(elementType);
                if (!listType.IsAssignableFrom(prop.PropertyType)) continue;

                var list = prop.GetValue(entity);
                if (list == null)
                {
                    prop.SetValue(entity, Activator.CreateInstance(listType));
                }
            }
        }
    }
}
