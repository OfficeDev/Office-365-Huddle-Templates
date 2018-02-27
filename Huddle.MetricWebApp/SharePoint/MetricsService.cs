/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.Models;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.SharePoint
{
    public class MetricsService
    {
        public static async Task<Metric[]> GetItemsAsync(int issueId)
        {
            var metric = new Metric();
            var filter = string.Format(
                @"<Eq>
                    <FieldRef Name='{0}' LookupId='TRUE'/>
                    <Value Type='Lookup'>{1}</Value> 
                </Eq>",
                SPLists.Metrics.Columns.Issue, issueId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var metricArray = items.Select(item => item.ToMetric())
                     .OrderBy(item => item.Id)
                     .ToArray();
                return metricArray;
            }
        }

        public static async Task<Metric> GetMetricById(int id)
        {
            var metric = new Metric();
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.Metrics.Columns.ID + @"'/>
                                    <Value Type='int'>" + id + @"</Value>
                                </Eq>
                            </Where>
                        </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var queryItem = items.FirstOrDefault();
                if (queryItem == null) return metric;

                metric = queryItem.ToMetric();
                return metric;
            }
        }

        public static async Task<Metric> InsertItemAsync(Metric item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var list = clientContext.Web.Lists.GetByTitle(SPLists.Metrics.Title);
                ListItem listItem = list.AddItem(new ListItemCreationInformation());
                listItem[SPLists.Metrics.Columns.Issue] = SharePointHelper.BuildSingleLookFieldValue(item.Issue.Id, item.Issue.Name);
                listItem[SPLists.Metrics.Columns.Title] = item.Name;
                listItem[SPLists.Metrics.Columns.State] = 1;
                listItem[SPLists.Metrics.Columns.TargetGoal] = item.TargetGoal;
                listItem[SPLists.Metrics.Columns.ValueType] = item.ValueType;
                listItem.Update();
                clientContext.Load(listItem);
                clientContext.ExecuteQuery();
                return listItem.ToMetric();
            }
        }

        public static async Task UpdateItemAsync(Metric item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.Metrics.Columns.ID + @"'/>
                                    <Value Type='int'>" + item.Id + @"</Value>
                                </Eq>
                            </Where>
                         </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var listItem = items.FirstOrDefault();
                if (listItem == null) return;

                listItem[SPLists.Metrics.Columns.Title] = item.Name;
                listItem[SPLists.Metrics.Columns.State] = item.State;
                listItem[SPLists.Metrics.Columns.TargetGoal] = item.TargetGoal;
                listItem[SPLists.Metrics.Columns.ValueType] = item.ValueType;
                listItem.Update();
                clientContext.Load(listItem);
                clientContext.ExecuteQuery();
            }
        }

        public static async Task<int> DeleteItemAsync(int id)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.Metrics.Columns.ID + @"'/>
                                    <Value Type='int'>" + id + @"</Value>
                                </Eq>
                            </Where>
                         </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var queryItem = items.FirstOrDefault();
                if (queryItem == null) return 0;

                queryItem.DeleteObject();
                clientContext.ExecuteQuery();
                return id;
            }
        }

        public static async Task DeleteMetricAndRelatedItemsAsync(int id)
        {
            await MetricValuesService.DeleteMetricValuesBMetricId(id);
            await ReasonsService.DeleteReasonAndValuesByMetricId(id);
            await DeletePlanTasksAsync(id);
            await DeleteItemAsync(id);
        }

        public static async Task DeleteMetricAndRelatedItemsByIssueId(int issueId)
        {
            var filter = string.Format(@"
                    <Eq>
                        <FieldRef Name='{0}' LookupId='TRUE'/>
                        <Value Type='Lookup'>{1}</Value> 
                    </Eq>",
                SPLists.Metrics.Columns.Issue, issueId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);

                for (int i = items.Count - 1; i > -1; i--)
                {
                    await DeleteMetricAndRelatedItemsAsync(items[i].Id);
                }
            }
        }

        public static async Task<bool> CalcMetricCount(List<Issue> issueList)
        {
            if (!issueList.Any()) return false;

            var relatedIssueIdsFilter = string.Format(
                    @"<In>
                        <FieldRef Name='{0}' LookupId='TRUE'/>
                        <Values>"
                        + string.Join("", issueList.Select(issue => @"<Value Type='Lookup'>" + issue.Id + "</Value>"))
                        + @"</Values>
                    </In>",
                    SPLists.Metrics.Columns.Issue);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", relatedIssueIdsFilter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var metricArray = items.Select(item => item.ToMetric())
                     .OrderBy(item => item.Id)
                     .ToArray();
                issueList.ForEach(issue =>
                {
                    issue.ActiveMetricCount = metricArray.Count(metric => metric.Issue.Id == issue.Id);
                });
                return true;
            }
        }

        public static async Task UpdateMetricStatus(int id)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.Metrics.Columns.ID + @"'/>
                                    <Value Type='int'>" + id + @"</Value>
                                </Eq>
                            </Where>
                         </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.Metrics.Title, query);
                var queryItem = items.FirstOrDefault();
                if (queryItem == null) return;

                var status = Convert.ToInt32(queryItem[SPLists.Metrics.Columns.State]);
                if (status == 0)
                    queryItem[SPLists.Metrics.Columns.State] = 1;
                else
                    queryItem[SPLists.Metrics.Columns.State] = 0;
                queryItem.Update();
                clientContext.ExecuteQuery();
            }
        }

        private static async Task DeletePlanTasksAsync(int metricId)
        {
            var filter = string.Format(
                @"<Eq>
                    <FieldRef Name='{0}' LookupId='TRUE'/>
                    <Value Type='Lookup'>{1}</Value> 
                </Eq>",
                SPLists.MetricIdeas.Columns.Metric, metricId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.MetricIdeas.Title, query);
                var graphServiceClient = await AuthenticationHelper.GetGraphServiceClientAsync();
                for (int i = items.Count - 1; i > -1; i--)
                {
                    var taskId = items[i].GetFieldValueStr(SPLists.MetricIdeas.Columns.TaskId);
                    var task = await graphServiceClient.Planner.Tasks[taskId]
                    .Request().GetAsync();
                    await graphServiceClient.Planner.Tasks[taskId]
                     .Request(new[] { new Microsoft.Graph.HeaderOption("If-Match", task.GetEtag()) })
                     .DeleteAsync();
                    items[i].DeleteObject();
                    clientContext.ExecuteQuery();
                }
            }
        }
    }
}
