using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;
using System;
using System.Linq;

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
                if (controller.ControllerType.Name.StartsWith("SingletonController"))
                {
                    // SingletonControllerMe => Me
                    var name = controller.ControllerType.Name.Substring("SingletonController".Length);
                    var singleton = _model.EntityContainer.Singletons().FirstOrDefault(s => s.Name == name);

                    if (singleton != null)
                    {
                        controller.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(singleton.Name))
                        });
                    }
                }
                else if (controller.ControllerType.IsGenericType)
                {
                    Type genericType = controller.ControllerType.GenericTypeArguments.First();
                    var entitySet = _model.EntityContainer.EntitySets().FirstOrDefault(e => e.EntityType().Name == genericType.Name);

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
