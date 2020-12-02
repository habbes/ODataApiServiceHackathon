using Hackathon2020.Poc01.Controllers;
using Hackathon2020.Poc01.Data;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Hackathon2020.Poc01.Lib
{
    public class DynamicControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        IEdmModel _model;
        public DynamicControllerFeatureProvider(IEdmModel model): base()
        {
            _model = model;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            Assembly targetAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name.Contains(DbContextConstants.Name));
            var targetTypes = targetAssembly.DefinedTypes;

            var entitySets = _model.EntityContainer.EntitySets();

            foreach (var entitySet in entitySets)
            {
                var type = targetTypes.First(t => t.Name == entitySet.EntityType().Name);
                feature.Controllers.Add(
                    typeof(DynamicControllerBase<>).MakeGenericType(type).GetTypeInfo()
                );
            }

            // Build Assembly
            AssemblyName assemblyName = new AssemblyName("SingletonsAssembly");
            //AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(assembly_Name, AssemblyBuilderAccess.RunAndSave);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule($"{assemblyName.Name}");

            foreach (var singleton in _model.EntityContainer.Singletons())
            {
                var type = targetTypes.First(t => t.Name == singleton.EntityType().Name);
                var baseType = typeof(DynamicControllerBase<>).MakeGenericType(type);

                var typeBuilder = module.DefineType($"SingletonController{singleton.Name}", TypeAttributes.Public | TypeAttributes.Class, baseType);

                var singletonControllerType = typeBuilder.CreateType();
                feature.Controllers.Add(singletonControllerType.GetTypeInfo());
            }
        }
    }
}
