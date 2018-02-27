/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Web.Mvc;
using System.Web.Routing;

namespace Huddle.MetricWebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "Account/{action}",
                defaults: new { controller = "Account" }
            );

            routes.MapRoute(
                 name: "Tab",
                 url: "Tab/{action}",
                 defaults: new { controller = "Tab", action = "Index" }
             );

            routes.MapRoute(
                name: "Admin",
                url: "Admin/{action}",
                defaults: new { controller = "Admin", action = "Index" }
            );

            routes.MapRoute(
                name: "HomeError",
                url: "Home/Error",
                defaults: new { controller = "Home", action = "Error" }
            );

            routes.MapRoute(
               name: "Default",
               url: "{*anything}",
               defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
           );
        }
    }
}
