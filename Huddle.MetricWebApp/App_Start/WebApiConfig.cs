/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Web.Http;

namespace Huddle.MetricWebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DoubleParamsApi",
                routeTemplate: "api/{controller}/{id1}/{id2}",
                defaults: new { id1 = RouteParameter.Optional, id2 = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
               name: "TrippleParamsApi",
               routeTemplate: "api/{controller}/{id1}/{id2}/{id3}",
               defaults: new { id1 = RouteParameter.Optional, id2 = RouteParameter.Optional, id3 = RouteParameter.Optional }
           );
        }
    }
}
