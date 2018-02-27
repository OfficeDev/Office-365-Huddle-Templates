/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Web.Mvc;

namespace Huddle.MetricWebApp.Infrastructure
{
    public class AddContentSecurityPolicyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!(filterContext.Result is ViewResult)) return;

            var host = filterContext.HttpContext.Request.Url.Host;

            // https://msdn.microsoft.com/en-us/microsoft-teams/prerequisites

            var response = filterContext.HttpContext.Response;
            // Only allow pages to be iframed by Microsoft Teams for extra security
            response.Headers.Add(
                "Content-Security-Policy",
                "frame-ancestors teams.microsoft.com *.teams.microsoft.com *.skype.com " + host);
            // For Internet Explorer 11 compatability
            response.Headers.Add(
                "X-Content-Security-Policy",
                "frame-ancestors teams.microsoft.com *.teams.microsoft.com " + host);
            // This header is deprecated but still respected by most browsers.
            response.Headers.Add(
                "X-Frame-Options",
                "ALLOW-FROM https://teams.microsoft.com/");

            base.OnActionExecuted(filterContext);
        }
    }
}
