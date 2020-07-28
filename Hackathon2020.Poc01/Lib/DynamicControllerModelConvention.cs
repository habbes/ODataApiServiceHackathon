using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Linq;
using System.Reflection;

namespace Hackathon2020.Poc01.Lib
{
    public class DynamicControllerModelConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType.IsGenericType)
                {
                    Type genericType = controller.ControllerType.GenericTypeArguments.First();
                    var dynamicControllerAttribute = genericType.GetCustomAttribute<DynamicControllerAttribute>();

                    if (dynamicControllerAttribute?.Route != null)
                    {
                        controller.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(dynamicControllerAttribute.Route)),
                        });
                    }
                }
            }
        }
    }
}
