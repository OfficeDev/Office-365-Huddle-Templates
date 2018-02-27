/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System.Configuration;

namespace Huddle.MetricWebApp
{
    public class Constants
    {
        public static readonly string AADClientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static readonly string AADClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public static readonly string AADClientCertThumbprint = ConfigurationManager.AppSettings["ida:ClientCertThumbprint"];
        public static readonly string AADTenantId = ConfigurationManager.AppSettings["ida:TenantId"];

        public static readonly string AADInstance = "https://login.microsoftonline.com/";
        public static readonly string Authority = AADInstance + "common";
        
        public static readonly string BaseSPSiteUrl = ConfigurationManager.AppSettings["BaseSPSiteUrl"];

        public static readonly string Admin = ConfigurationManager.AppSettings["Admin"];

        public static class Resources
        {
            public static readonly string AADGraph = "https://graph.windows.net";
            public static readonly string MSGraph = "https://graph.microsoft.com";
            public static readonly string SharePoint = "https://cand3.sharepoint.com";
        }
    }
}
