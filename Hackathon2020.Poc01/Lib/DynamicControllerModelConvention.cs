using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;
using System;
using System.Linq;
using System.Reflection;

namespace Hackathon2020.Poc01.Lib
{
    public class DynamicControllerModelConvention : IApplicationModelConvention
    {
        IEdmModel _model;
        public DynamicControllerModelConvention(IEdmModel model): base()
        {
            _model = model;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType.IsGenericType)
                {
                    Type genericType = controller.ControllerType.GenericTypeArguments.First();
                    //var dynamicControllerAttribute = genericType.GetCustomAttribute<DynamicControllerAttribute>();
                    var entitySet = _model.EntityContainer.EntitySets().FirstOrDefault(e => e.EntityType().Name == genericType.Name);

                    //if (dynamicControllerAttribute?.Route != null)
                    //{
                    //    controller.Selectors.Add(new SelectorModel
                    //    {
                    //        AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(dynamicControllerAttribute.Route)),
                    //    });
                    //}

                    if (entitySet != null)
                    {
                        controller.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(entitySet.Name)),
                        });
                    }
                }
            }
        }
    }
}
