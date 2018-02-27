/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.Web;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Infrastructure
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated) return false;
            return httpContext.User.Identity.Name.Equals(
                Constants.Admin, 
                StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ContentResult
                {
                    Content = "Only admin could access this page."
                };
            }
            else
                base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
