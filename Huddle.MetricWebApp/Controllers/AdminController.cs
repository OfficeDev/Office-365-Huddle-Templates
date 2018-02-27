/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.SharePoint;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Huddle.MetricWebApp.Controllers
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

        [AdminOnly]
        public ActionResult ClearData()
        {
            return View();
        }

        [AdminOnly, HttpPost]
        public async Task<ActionResult> ClearDataPost()
        {
            try
            {
                var clientContext = await AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application);
                var service = new DataClearService(clientContext);
                service.ClearListItems();
                TempData["ClearDataResult"] = "Data was successfully cleared.";
            }
            catch (Exception ex)
            {
                TempData["ClearDataResult"] = "Data clear failed: " + ex.Message;
            }
            return RedirectToAction("ClearData");
        }
    }
}
