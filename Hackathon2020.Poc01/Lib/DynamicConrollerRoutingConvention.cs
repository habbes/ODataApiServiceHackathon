using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Hackathon2020.Poc01.Lib
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicControllerRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw new ArgumentNullException("routeContext");
            }

            Microsoft.AspNet.OData.Routing.ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            if (odataPath == null)
            {
                throw new ArgumentNullException("odataPath");
            }

            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(odataPath);
            if (controllerResult != null)
            {
                // Get a IActionDescriptorCollectionProvider from the global service provider.
                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
                Contract.Assert(actionCollectionProvider != null);

                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.AttributeRouteInfo != null && c.AttributeRouteInfo.Template == controllerResult.ControllerName);

                if (actionDescriptors != null)
                {
                    string actionName = SelectAction(routeContext, controllerResult, actionDescriptors);
                    if (!String.IsNullOrEmpty(actionName))
                    {
                        return actionDescriptors.Where(
                            c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        internal static SelectControllerResult SelectControllerImpl(Microsoft.AspNet.OData.Routing.ODataPath odataPath)
        {
            // entity set
            EntitySetSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetSegment;
            if (entitySetSegment != null)
            {
                return new SelectControllerResult(entitySetSegment.EntitySet.Name, null);
            }

            // singleton
            SingletonSegment singletonSegment = odataPath.Segments.FirstOrDefault() as SingletonSegment;
            if (singletonSegment != null)
            {
                return new SelectControllerResult(singletonSegment.Singleton.Name, null);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeContext"></param>
        /// <param name="controllerResult"></param>
        /// <param name="actionDescriptors"></param>
        /// <returns></returns>
        public string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            return SelectActionImpl(routeContext, controllerResult, actionDescriptors);
        }

        private string SelectActionImpl(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            Microsoft.AspNet.OData.Routing.ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            if (odataPath.PathTemplate.EndsWith("$ref"))
            {
                if (HttpMethods.IsDelete(request.Method))
                {
                    return actionDescriptors.FindMatchingAction("DeleteRef");
                }
                else if (HttpMethods.IsPost("$ref") || HttpMethods.IsPut("$ref"))
                {
                    return actionDescriptors.FindMatchingAction("CreateRef");
                }
            }
            else if (odataPath.PathTemplate.EndsWith("$count"))
            {
                // $count is only supported for GET requests
                if (HttpMethods.IsGet(request.Method))
                {
                    return actionDescriptors.FindMatchingAction("Get");
                }
            }
            else if (HttpMethods.IsGet(request.Method))
            {
                // A single Get method will handle all Get requests.
                // It should contain the logic for traversing potenially nested path
                // and extract the relevant keys and navigation properties
                return actionDescriptors.FindMatchingAction("Get");
            }
            // TODO: we currently do not support nested paths for Create/Update/Delete requests
            // TODO: verify whether we support casts
            else if (HttpMethods.IsPost(request.Method))
            {
                if (odataPath.PathTemplate == "~/entityset" ||
                    odataPath.PathTemplate == "~/entityset/cast")
                {
                    return actionDescriptors.FindMatchingAction("Post");
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast" ||
                odataPath.PathTemplate == "~/singleton" ||
                odataPath.PathTemplate == "~/singleton/cast")
            {

                if (HttpMethods.IsPut(request.Method))
                {
                    return actionDescriptors.FindMatchingAction("Put");
                }
                else if (HttpMethods.IsPatch(request.Method))
                {
                    // TODO: we should filter support for navigation properties / nested paths
                    return actionDescriptors.FindMatchingAction("Patch");
                }
                else if (HttpMethods.IsDelete(request.Method))
                {
                    // TODO: we should filter support for navigation properties / nested paths
                    return actionDescriptors.FindMatchingAction("Delete");
                }
            }

            return null;
        }
    }

    internal static class WebApiActionExtensions
    {
        internal static bool Contains(this IEnumerable<ControllerActionDescriptor> actionDescriptors, string name)
        {
            return actionDescriptors.Any(a => String.Equals(a.ActionName, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string FindMatchingAction(this IEnumerable<ControllerActionDescriptor> actionDescriptors, params string[] targetActionNames)
        {
            foreach (string targetActionName in targetActionNames)
            {
                if (actionDescriptors.Contains(targetActionName))
                {
                    return targetActionName;
                }
            }

            return null;
        }
    }
}
