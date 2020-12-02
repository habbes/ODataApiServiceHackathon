using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    internal static class EntityHelpers
    {
        /// <summary>
        /// Sets each <see cref="List{T}"/> property that is <see cref="null"/> to an empty list.
        /// </summary>
        /// <param name="entity"></param>
        public static void InitializeListProperties(this object entity)
        {
            var properties = entity.GetType().GetProperties();
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
