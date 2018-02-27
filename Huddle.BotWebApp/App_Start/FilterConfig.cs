/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Infrastructure;
using System.Web.Mvc;

namespace Huddle.BotWebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LogErrorAttribute(), 100);
            filters.Add(new HandleErrorAttribute(), 1500);
        }
    }
}
