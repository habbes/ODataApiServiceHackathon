using Hackathon2020.Poc01.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hackathon2020.Poc01.Lib
{
    public class DynamicControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            Assembly targetAssembly = typeof(DynamicControllerFeatureProvider).Assembly;
            IEnumerable<Type> targetTypes = targetAssembly.GetExportedTypes().Where(d => d.GetCustomAttributes(typeof(DynamicControllerAttribute), true).Any());

            foreach (Type type in targetTypes)
            {
                feature.Controllers.Add(
                    typeof(DynamicControllerBase<>).MakeGenericType(type).GetTypeInfo()
                );
            }
        }
    }
}
