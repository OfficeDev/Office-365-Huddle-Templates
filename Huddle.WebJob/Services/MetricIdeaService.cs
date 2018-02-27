/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Huddle.WebJob.Models;
using Huddle.Common;

namespace Huddle.WebJob.Services
{
    class MetricIdeaService
    {
        private ClientContext spClientContext;

        public MetricIdeaService(ClientContext spClientContext)
        {
            this.spClientContext = spClientContext;
        }

        public bool UpdateIdeaInMetricIdeaList(Idea oldIdea, Idea newIdea)
        {
            var items = GetMetricIdeaItemsByTaskID(oldIdea.Id);
            if (items.Count() == 0)
            {
                LogNoListItemForIdea(oldIdea);
                return false;
            }

            foreach (var item in items)
            {
                item[SPLists.MetricIdeas.Columns.TaskId] = newIdea.Id;
                item[SPLists.MetricIdeas.Columns.TaskURL] = newIdea.Url;
                item.Update();
                spClientContext.ExecuteQuery();
                LogService.LogInfo($"Updated item {item.Id} in SharePoint list {SPLists.MetricIdeas.Title}.");
            }
            return true;
        }

        public bool DeleteIdeaInMetricIdeaList(Idea idea)
        {
            var items = GetMetricIdeaItemsByTaskID(idea.Id);
            if (items.Count() == 0)
            {
                LogNoListItemForIdea(idea);
                return false;
            }

            foreach (var item in items)
            {
                item.DeleteObject();
                LogService.LogInfo($"Deleted item {item.Id} in SharePoint list {SPLists.MetricIdeas.Title}.");
            }
            spClientContext.ExecuteQuery();
            return true;
        }

        public async Task SyncMetricIdeaList()
        {
            var teamService = new TeamsService();
            var teams = await teamService.GetJoinedTeamsAsync();
            var plannerService = new PlannerService(teamService);
            var globalTeam = await teamService.GetGlobalTeamAsync();
            var globalIdeas = globalTeam == null ? new Idea[0] : (await plannerService.GetIdeasInTeamAsync(globalTeam))
                .OrderBy(idea => idea.Id)
                .ToArray();

            var allIdeas = new List<Idea>();
            foreach (var team in teams)
            {
                var operation = $"Sync ideas in team {team?.DisplayName}";
                LogService.LogOperationStarted(operation);
                var ideas = await plannerService.GetIdeasInTeamAsync(team);
                allIdeas.AddRange(ideas);
                ideas = ideas.Union(globalIdeas)
                    .OrderBy(s => s.Id);
                var items = GetMetricIdeaItemsByTeamID(team.Id)
                    .OrderBy(item => item[SPLists.MetricIdeas.Columns.TaskId]);
                try
                {
                    SyncIdeasAndListItems(ideas, items);
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
                LogService.LogOperationEnded(operation);
            }

            // Sync ideas whose metric is other
            {
                var operation = $"Sync ideas whose metric is other";
                LogService.LogOperationStarted(operation);
                var items = GetOtherMetricIdeaItems();
                var ideaIds = items.Select(i => i[SPLists.MetricIdeas.Columns.TaskId] as string).ToArray();
                var ideas = allIdeas.Where(i => ideaIds.Contains(i.Id));
                SyncIdeasAndListItems(ideas, items);
                LogService.LogOperationEnded(operation);
            }
        }

        private void SyncIdeasAndListItems(IEnumerable<Idea> ideas, IEnumerable<ListItem> items)
        {
            var anyItemUpdated = false;
            var itemsArray = items.ToArray();
            var count = itemsArray.Count();
            LogService.LogInfo($"Items:{count},Ideas:{ideas.Count()}");
            for (var i = 0; i < count; ++i)
            {
                var item = itemsArray[i];
                var ideaOfItem = ideas.Where(idea => idea.Id.Equals(item[SPLists.MetricIdeas.Columns.TaskId])).FirstOrDefault();
                var objTaskStartDate = item[SPLists.MetricIdeas.Columns.TaskStartDate];
                var taskStartDate = objTaskStartDate is DateTime ? new DateTimeOffset?((DateTime)objTaskStartDate) : null;
                if (ideaOfItem == null)
                {
                    item.DeleteObject();
                    anyItemUpdated = true;
                    LogService.LogInfo($"Deleting SharePoint list item {item.Id}.");
                }
                else
                {
                    if (ideaOfItem.Title.Equals(item[SPLists.MetricIdeas.Columns.TaskName]) &&
                        ideaOfItem.BucketName.Equals(item[SPLists.MetricIdeas.Columns.TaskStatus]) &&
                        ideaOfItem.StartDateTime.Equals(taskStartDate))
                        continue;

                    if (!ideaOfItem.Title.Equals(item[SPLists.MetricIdeas.Columns.TaskName]))
                        item[SPLists.MetricIdeas.Columns.TaskName] = ideaOfItem.Title;

                    if (!ideaOfItem.BucketName.Equals(item[SPLists.MetricIdeas.Columns.TaskStatus]))
                    {
                        item[SPLists.MetricIdeas.Columns.TaskStatus] = ideaOfItem.BucketName;
                        if (ideaOfItem.BucketName == Constants.CompletedBucket)
                            item[SPLists.MetricIdeas.Columns.TaskCompletedDate] = DateTime.UtcNow;
                    }
                    if (!ideaOfItem.StartDateTime.Equals(taskStartDate))
                        item[SPLists.MetricIdeas.Columns.TaskStartDate] = ideaOfItem.StartDateTime;
                    item.Update();
                    anyItemUpdated = true;
                    LogService.LogInfo($"Updating SharePoint list item {item.Id}.");
                }
            }
            if (anyItemUpdated)
            {
                spClientContext.ExecuteQuery();
                LogService.LogInfo($"Committed the update of SharePoint list.");
            }
        }

        private IEnumerable<ListItem> GetMetricIdeaItemsByTeamID(string teamId)
        {
            var metricItems = GetMetricItemsByTeamID(teamId);
            if (metricItems.Count() == 0)
                return new ListItem[0];

            var condition = new StringBuilder(@"<In>");
            condition.AppendLine($"<FieldRef Name='{SPLists.MetricIdeas.Columns.Metric}' LookupId='TRUE' />");
            condition.AppendLine("<Values>");
            foreach (var item in metricItems)
                condition.AppendLine($"<Value Type='Lookup'>{item.Id}</Value>");
            condition.AppendLine("</Values>");
            condition.AppendLine("</In>");

            var query = new CamlQuery();
            query.ViewXml =
                @"<View>
                    <Query>
                        <Where>" +
                            condition + @"
                        </Where>
                    </Query>
                </View>";

            return GetSharePointListItems(SPLists.MetricIdeas.Title, query);
        }

        /// <summary>
        /// Get MetricIdea items whose metric is other
        /// </summary>
        private IEnumerable<ListItem> GetOtherMetricIdeaItems()
        {
            var query = new CamlQuery();
            query.ViewXml =
                @"<View>
                    <Query>
                        <Where>
                            <IsNull>
                                <FieldRef Name='" + SPLists.MetricIdeas.Columns.Metric + @"'></FieldRef>
                            </IsNull>
                        </Where>
                    </Query>
                </View>";
            return GetSharePointListItems(SPLists.MetricIdeas.Title, query);
        }

        private ListItemCollection GetMetricIdeaItemsByTaskID(string taskId)
        {
            var query = new CamlQuery();
            query.ViewXml =
                @"<View>
                    <Query>
                        <Where>
                            <Eq>
                                <FieldRef Name='" + SPLists.MetricIdeas.Columns.TaskId + @"'/>
                                <Value Type='Text'>" + taskId + @"</Value>
                            </Eq>
                        </Where>
                    </Query>
                </View>";
            return GetSharePointListItems(SPLists.MetricIdeas.Title, query);
        }

        private IEnumerable<ListItem> GetMetricItemsByTeamID(string teamId)
        {
            var query = new CamlQuery();
            query.ViewXml =
                @"<View>
                    <Query>
                        <Where>
                            <Eq>
                                <FieldRef Name='" + SPLists.Issues.Columns.TeamId + @"'/>
                                <Value Type='Text'>" + teamId + @"</Value>
                            </Eq>
                        </Where>
                    </Query>
                </View>";
            var issues = GetSharePointListItems(SPLists.Issues.Title, query).Select(s => s.Id).ToArray();
            if (issues.Length == 0)
                return new ListItem[0];

            //Get Metrics
            var condition = new StringBuilder(@"<In>");
            condition.AppendLine($"<FieldRef Name='{SPLists.Metrics.Columns.Issue}' LookupId='TRUE' />");
            condition.AppendLine("<Values>");
            foreach (var id in issues)
                condition.AppendLine($"<Value Type='Lookup'>{id}</Value>");
            condition.AppendLine("</Values>");
            condition.AppendLine("</In>");

            query = new CamlQuery();
            query.ViewXml =
                @"<View>
                    <Query>
                        <Where>" +
                            condition + @"
                        </Where>
                    </Query>
                </View>";
            return GetSharePointListItems(SPLists.Metrics.Title, query);
        }

        private ListItemCollection GetSharePointListItems(string listTitle, CamlQuery query)
        {
            var web = spClientContext.Site.RootWeb;
            var list = web.Lists.GetByTitle(listTitle);
            var items = list.GetItems(query);
            spClientContext.Load(items);
            spClientContext.ExecuteQuery();
            return items;
        }

        private void LogNoListItemForIdea(Idea idea)
        {
            LogService.LogInfo($"There are no items with TaskID {idea.Id} in SharePoint list {SPLists.MetricIdeas.Title}.");
        }
    }
}
