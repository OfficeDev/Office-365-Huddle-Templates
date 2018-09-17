﻿/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace Huddle.BotWebApp.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn(string redirectUri = "/")
        {
            //if (HttpContext.Request.Url.ToString().ToLower().Contains("localhost"))
            //    redirectUri = ConfigurationManager.AppSettings["LocalHostNGrokURI"] + redirectUri;
            // Send an OpenID Connect sign-in request.
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public ActionResult SignInCallback()
        {
            return View();
        }

        public void SignOut()
        {
            string callbackUrl;
            //if (HttpContext.Request.Url.ToString().ToLower().Contains("localhost"))
            //    callbackUrl = ConfigurationManager.AppSettings["LocalHostNGrokURI"] + "/Account/SignOutCallback";
            //else
            callbackUrl = Url.Action("SignOutCallback", "Account", routeValues: null, protocol: Request.Url.Scheme);
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
