/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Huddle.BotWebApp.Infrastructure
{
    public class LogErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            LogException(filterContext.Exception);
        }

        private void LogException(Exception exception)
        {
            var file = HostingEnvironment.MapPath("/App_Data/Logs/" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log");
            var contents = $"[{DateTime.UtcNow.ToShortTimeString()}][{exception.GetType().Name}]{exception.Message}\r\n{exception.StackTrace}";
            File.AppendAllText(file, contents);
        }
    }
}
