/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.Models;
using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.SharePoint
{
    public class QueryService
    {
        public static async Task<IQueryResult[]> QueryItemsAsync(int state, string teamId, string key)
        {
            var issueFilter = string.Format(
                @"<And>
                    <And>
                        <Eq>
                            <FieldRef Name='{0}' />
                            <Value Type='Choice'>{1}</Value>
                        </Eq>
                        <Eq>
                            <FieldRef Name='{2}' />
                            <Value Type='Text'>{3}</Value>
                        </Eq>
                    </And>
                    <Contains>
                        <FieldRef Name='{4}' />
                        <Value Type='Text'>{5}</Value>
                    </Contains>
                </And>",
                SPLists.Issues.Columns.State, state,
                SPLists.Issues.Columns.TeamId, teamId,
                SPLists.Issues.Columns.Title, key);
            var issueQuery = new CamlQuery();
            issueQuery.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", issueFilter);

            var metricFilter = string.Format(
                @"<Contains>
                    <FieldRef Name='{0}' />
                    <Value Type='Text'>{1}</Value>
                </Contains>",
                SPLists.Metrics.Columns.Title, key);
            var metricQuery = new CamlQuery();
            metricQuery.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", metricFilter);

            var reasonFilter = string.Format(
                @"<Contains>
                    <FieldRef Name='{0}' />
                    <Value Type='Text'>{1}</Value>
                </Contains>",
                SPLists.Reasons.Columns.Title, key);
            var reasonQuery = new CamlQuery();
            reasonQuery.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", reasonFilter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var result = new List<IQueryResult>();
                var issues = clientContext.GetItems(SPLists.Issues.Title, issueQuery);
                var issueArray = issues.Select(item => item.ToIssue())
                    .OrderBy(item => item.Id)
                    .ToArray();

                var metrics = clientContext.GetItems(SPLists.Metrics.Title, metricQuery);
                var metricArray = metrics.Select(item => item.ToMetric())
                     .OrderBy(item => item.Id)
                     .ToArray();

                var reasons = clientContext.GetItems(SPLists.Reasons.Title, reasonQuery);
                var reasonArray = reasons.Select(item => item.ToReason())
                     .OrderBy(item => item.Id)
                     .ToArray();

                if (reasonArray.Length > 0)
                {
                    var relatedMetricQuery = new CamlQuery();
                    var metricIdsRelated = reasonArray.Select(reason => reason.Metric.Id);

                    var relatedMetricIdsFilter = string.Format(
                        @"<In>
                            <FieldRef Name='{0}' />
                            <Values>" +
                                string.Join("", metricIdsRelated.Select(metricId => { return @"<Value Type='Text'>" + metricId + "</Value>"; }))
                            + @"</Values>
                        </In>",
                        SPLists.Metrics.Columns.ID, key);
                    relatedMetricQuery.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", relatedMetricIdsFilter);
                    var relatedMetricArray = clientContext.GetItems(SPLists.Metrics.Title, relatedMetricQuery)
                        .Select(item => item.ToMetric())
                        .OrderBy(item => item.Id)
                        .ToArray();
                    foreach (var reason in reasonArray)
                        reason.Metric.Issue = relatedMetricArray.First(metric => metric.Id == reason.Metric.Id).Issue;
                }

                result.AddRange(issueArray);
                result.AddRange(metricArray);
                result.AddRange(reasonArray);
                return result.ToArray();
            }
        }
    }
}
