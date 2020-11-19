using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoBogus;
using Bogus;

namespace Hackathon2020.Poc01.Lib.Seeder
{
    public class DataSeeder
    {
        IEdmModel model;
        DbContext context;
        IEnumerable<TypeInfo> types;

        public DataSeeder(IEdmModel model, DbContext context, IEnumerable<TypeInfo> types)
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

            await context.SaveChangesAsync();
        }

        private void SeedEntitySet(IEdmEntitySet entitySet)
        {
            var edmEntityType = entitySet.EntityType();
            var entityType = types.First(t => t.Name == edmEntityType.Name);

            var dbSet = context.GetType().GetMethod("Set").MakeGenericMethod(entityType).Invoke(context, Array.Empty<object>());
            var addMethod = dbSet.GetType().GetMethod("Add");
            var generator = GetGeneratorForType(entityType);

            int count = 100;

            for (int i = 0; i < count; i++)
            {
                var obj = generator.Invoke(null, new object[] { });
                addMethod.Invoke(dbSet, new[] { obj });
            }
        }

        private MethodInfo GetGeneratorForType(Type type)
        {
            var argType = typeof(Action<>).MakeGenericType(typeof(IAutoGenerateConfigBuilder));
            var generatMethod = typeof(AutoFaker).GetMethod("Generate", new[] { argType });
            var generateType = generatMethod.MakeGenericMethod(type);
            return generateType;
        }
    }
}
