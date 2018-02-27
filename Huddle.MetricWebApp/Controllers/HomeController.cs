/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Diagnostics;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string teamId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Debugger.IsAttached
                    ? RedirectToAction("SignIn", "Account", new { redirectUri = "/?teamId=" + teamId })
                    : RedirectToAction("SignIn", "Tab", new { teamId = teamId });
            }
            if (string.IsNullOrEmpty(teamId))
                return RedirectToAction("Error");
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
