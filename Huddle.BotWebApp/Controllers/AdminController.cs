/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Web.Mvc;

namespace Huddle.BotWebApp.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Consent()
        {
            return RedirectToAction("SignIn", "Account", new { prompt = "admin_consent", redirectUri = "/Admin/Consented" });
        }

        public ActionResult Consented(string code)
        {
            return View();
        }
    }
}
