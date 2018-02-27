/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Microsoft.Azure;

namespace Huddle.WebJob
{
    public class Constants
    {
        public static readonly string ShareableBucket = "Shareable";
        public static readonly string CompletedBucket = "Completed";
        public static readonly string GlobalTeam = CloudConfigurationManager.GetSetting("GlobalTeam");
        public static readonly string GlobalTaskLifeSpan = CloudConfigurationManager.GetSetting("GlobalTaskLifeSpan");


        public static readonly string AADClientId = CloudConfigurationManager.GetSetting("ida:ClientId");
        public static readonly string AADClientCertThumbprint = CloudConfigurationManager.GetSetting("ida:ClientCertThumbprint");
        public static readonly string AADTenantId = CloudConfigurationManager.GetSetting("ida:TenantId");

        public static readonly string AADInstance = "https://login.microsoftonline.com/";
        public static readonly string Authority = AADInstance + AADTenantId;

        public static readonly string BaseSPSiteUrl = CloudConfigurationManager.GetSetting("BaseSPSiteUrl");
        
        public static class LogicAppUrls
        {
            public static readonly string GetJoinedTeams = CloudConfigurationManager.GetSetting("LogicApp_GetJoinedTeams");
            public static readonly string ListPlans = CloudConfigurationManager.GetSetting("LogicApp_ListPlans");
            public static readonly string CreateTask = CloudConfigurationManager.GetSetting("LogicApp_CreateTask");
            public static readonly string DeleteTask = CloudConfigurationManager.GetSetting("LogicApp_DeleteTask");
            public static readonly string ListBuckets = CloudConfigurationManager.GetSetting("LogicApp_ListBuckets");
            public static readonly string ListBucketTasks = CloudConfigurationManager.GetSetting("LogicApp_ListBucketTasks");
            public static readonly string ListPlanTasks = CloudConfigurationManager.GetSetting("LogicApp_ListPlanTasks");
            public static readonly string GetTaskDetails = CloudConfigurationManager.GetSetting("LogicApp_GetTaskDetails");
        }
    }
}
