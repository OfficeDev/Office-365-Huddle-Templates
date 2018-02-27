/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.WebJob.Infrastructure;
using Huddle.WebJob.Services;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;

namespace Huddle.WebJob
{
    public class Functions
    {
        public async static Task MoveShareableIdeas([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
        {
            var operation = "Move shareable ideas";
            LogService.LogOperationStarted(operation);
            using (var spClientContext = await AuthenticationHelper.GetSharePointClientAppOnlyContextAsync())
            {
                var plannerService = new PlannerService(spClientContext);
                try
                {
                    await plannerService.MoveShareableIdeas();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            }
            LogService.LogOperationEnded(operation);
        }

        public async static Task RemoveObsoleteIdeas([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
        {
            var operation = "Remove obsolete ideas";
            LogService.LogOperationStarted(operation);
            using (var spClientContext = await AuthenticationHelper.GetSharePointClientAppOnlyContextAsync())
            {
                var plannerService = new PlannerService(spClientContext);
                try
                {
                    await plannerService.ReoveObsoleteIdeas();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            }
            LogService.LogOperationEnded(operation);
        }

        public async static Task SyncMetricIdeaList([TimerTrigger("0 30 * * * *")] TimerInfo timerInfo)
        {
            var operation = $"Sync SharePoint list {SPLists.MetricIdeas.Title}";
            LogService.LogOperationStarted($"{operation} started.");
            using (var spClientContext = await AuthenticationHelper.GetSharePointClientAppOnlyContextAsync())
            {
                var metricIdeaService = new MetricIdeaService(spClientContext);
                try
                {
                    await metricIdeaService.SyncMetricIdeaList();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            }
            LogService.LogOperationEnded(operation);
        }
    }
}
