using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoBogus;
using Bogus;
using DataLib;
using System.Collections;

namespace Hackathon2020.Poc01.Lib.Seeder
{
    public class DataSeeder
    {
        IEdmModel model;
        IDataStore context;
        IEnumerable<TypeInfo> types;

        public DataSeeder(IEdmModel model, IDataStore context, IEnumerable<TypeInfo> types)
        {
            this.model = model;
            this.context = context;
            this.types = types;
        }
        public async Task SeedData()
        {
            foreach (var entitySet in model.EntityContainer.EntitySets())
            {
                SeedEntitySet(entitySet);
            }

            foreach (var entitySet in model.EntityContainer.EntitySets())
            {
                AssignNavigationPropertiesFor(entitySet);
            }

            await context.SaveChangesAsync();
        }

        private void SeedEntitySet(IEdmEntitySet entitySet)
        {
            var edmEntityType = entitySet.EntityType();
            var entityType = types.First(t => t.Name == edmEntityType.Name);

            var dbSet = context.GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(context, Array.Empty<object>());
            var addMethod = dbSet.GetType().GetMethod("Add");
            var generator = GetGeneratorForType(entityType);

            int count = 20;

            var generatedKeys = new HashSet<object>();

            for (int i = 0; i < count; i++)
            {
                var obj = GenerateValue(generator);
                AssignUniqueKey(obj, entityType, edmEntityType, generatedKeys);
                addMethod.Invoke(dbSet, new[] { obj });
            }
        }

        private void AssignNavigationPropertiesFor(IEdmEntitySet entitySet)
        {
            var edmEntityType = entitySet.EntityType();
            var entityType = GetEntityType(edmEntityType.Name);
            var dbSet = GetDataSet(entityType);
            var enumerable = dbSet as IEnumerable;
            

            //foreach (var edmNavProp in edmEntityType.DeclaredNavigationProperties())
            //{
            //    Console.WriteLine(edmNavProp.Name);
            //}
            foreach (var entity in enumerable)
            {
                foreach (var navProp in edmEntityType.NavigationProperties())
                {
                    if (navProp.ContainsTarget) continue;

                    var isCollection = navProp.Type.IsCollection();

                    if (isCollection)
                    {
                        AssignCollectionNavigationPropertyForEntity(entity, entityType, navProp);
                    }
                    else
                    {
                        AssignSingleNavigationPropertyForEntity(entity, entityType, navProp);
                    }
                }
            }

            
        }

        private void AssignSingleNavigationPropertyForEntity(object entity, Type entityType, IEdmNavigationProperty navProp)
        {
            var prop = entityType.GetProperty(navProp.Name.Split('/').Last());
            prop.SetValue(entity, null);
            var targetDbSet = GetDataSet(prop.PropertyType) as IEnumerable;
            prop.SetValue(entity, GetRandomItem(targetDbSet));
        }

        private void AssignCollectionNavigationPropertyForEntity(object entity, Type entityType, IEdmNavigationProperty navProp)
        {
            var prop = entityType.GetProperty(navProp.Name.Split('/').Last());
            var list = prop.GetValue(entity);
            var clearMethod = prop.PropertyType.GetMethod("Clear");
            clearMethod.Invoke(list, Array.Empty<object>());

            var addMethod = prop.PropertyType.GetMethod("Add");

            var targetDbSet = GetDataSet(prop.PropertyType.GetGenericArguments()[0]) as IEnumerable;
            int maxItems = 10;
            int addedCount = 0;

            var random = new Random();

            foreach (var item in targetDbSet)
            {
                if (random.Next(0, 10) < 3) // 0.3 chance of adding an item
                {
                    addMethod.Invoke(list, new[] { item });
                    ++addedCount;

                    if (addedCount == maxItems)
                    {
                        break;
                    }
                }
            }
        }

        private object GetRandomItem(IEnumerable dataSet)
        {
            var random = new Random();
            var index = random.Next(0, 100);
            int i = 0;
            foreach (var item  in dataSet)
            {
                if (i == index) return item;
                ++i;
            }

            return null;
        }

        private object GetDataSet(Type entityType)
        {
            return context.GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(context, Array.Empty<object>());
        }

        private Type GetEntityType(string name)
        {
            return types.First(t => t.Name == name);
        }

        private MethodInfo GetGeneratorForType(Type type)
        {
            var argType = typeof(Action<>).MakeGenericType(typeof(IAutoGenerateConfigBuilder));
            var generatMethod = typeof(AutoFaker).GetMethod("Generate", new[] { argType });
            var generateType = generatMethod.MakeGenericMethod(type);
            return generateType;
        }

        private object GenerateValue(MethodInfo generator)
        {
            return generator.Invoke(null, new object[] { null });
        }

        private object GenerateUniqueValue(Type type, ISet<object> existingKeys)
        {
            var generator = GetGeneratorForType(type);
            var value = GenerateValue(generator);
            while (existingKeys.Contains(value))
            {
                value = GenerateValue(generator);
            }

            existingKeys.Add(value);
            return value;
        }

        private void AssignUniqueKey(object obj, Type entityType, IEdmEntityType edmEntityType, ISet<object> existingKeys)
        {
            // We only assign unique key for the first key property.
            // Even if it's a composite key, if one of its values is unique, then the entire key is unique
            var keyPropName = edmEntityType.Key().First().Name;
            var prop = entityType.GetProperty(keyPropName);
            var value = GenerateUniqueValue(prop.PropertyType, existingKeys);
            prop.SetValue(obj, value);
        }
    }
}
