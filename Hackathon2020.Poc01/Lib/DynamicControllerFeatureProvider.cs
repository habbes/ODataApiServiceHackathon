using Hackathon2020.Poc01.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            //Assembly targetAssembly = typeof(DynamicControllerFeatureProvider).Assembly;
            Assembly targetAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name.Contains("SampleModel"));
            //IEnumerable<Type> targetTypes = targetAssembly.GetExportedTypes().Where(d => d.GetCustomAttributes(typeof(DynamicControllerAttribute), true).Any());
            var targetTypes = targetAssembly.DefinedTypes;

            var entitySets = _model.EntityContainer.EntitySets();
            foreach (var entitySet in entitySets)
            {
                var type = targetTypes.First(t => t.Name == entitySet.EntityType().Name);
                feature.Controllers.Add(
                    typeof(DynamicControllerBase<>).MakeGenericType(type).GetTypeInfo()
                );
            }
            //foreach (Type type in targetTypes)
            //{
            //    feature.Controllers.Add(
            //        typeof(DynamicControllerBase<>).MakeGenericType(type).GetTypeInfo()
            //    );
            //}
        }
    }
}
