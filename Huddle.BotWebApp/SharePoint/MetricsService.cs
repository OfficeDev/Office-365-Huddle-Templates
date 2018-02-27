/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.BotWebApp.Models;
using Huddle.BotWebApp.Utils;
using Huddle.Common;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using System.Linq;
using System.Threading.Tasks;
using SP = Microsoft.SharePoint.Client;

namespace Huddle.BotWebApp.SharePoint
{
    public class MetricsService
    {
        private SP.ClientContext clientContext;

        public MetricsService(SP.ClientContext clientContext)
        {
            this.clientContext = clientContext;
        }

        public async Task<Metric[]> GetActiveMetricsAsync(string teamId)
        {
            var filter = string.Format(@"<And>
                    <Eq>
                        <FieldRef Name='{0}' />
                        <Value Type='Choice'>{1}</Value>
                    </Eq>
                    <Eq>
                        <FieldRef Name='{2}' />
                        <Value Type='Text'>{3}</Value>
                    </Eq>
                </And>",
               SPLists.Issues.Columns.State,
               SPLists.Issues.States.Active,
               SPLists.Issues.Columns.TeamId,
               teamId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await AuthenticationHelper.GetAppOnlySharePointClientContextAsync())
            {
                var issues = clientContext.GetItems(SPLists.Issues.Title, query)
                    .Select(s => s[SPLists.Issues.Columns.ID] as int?)
                    .ToArray();
                if (issues.Length > 0)
                {
                    filter = string.Format(@"<And>
                            <Eq>
                                <FieldRef Name='{0}' />
                                <Value Type='Choice'>{1}</Value>
                            </Eq>
                            <In>
                                <FieldRef Name='{2}' LookupId='True' />
                                <Values><Value Type='LookUp'>{3}</Value></Values>
                            </In>
                        </And>",
                       SPLists.Metrics.Columns.State,
                       SPLists.Issues.States.Active,
                       SPLists.Metrics.Columns.Issue,
                       string.Join("</Value><Value Type='LookUp'>", issues));

                    query = new CamlQuery();
                    query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);
                    var metrics = clientContext.GetItems(SPLists.Metrics.Title, query);
                    return metrics.Select(ToMetric).ToArray();
                }
                else
                    return new Metric[0];
            }
        }

        public async Task CreateMetricIdeaAsync(int? metricId, PlannerTask task, string bucket, string taskURL)
        {
            using (var clientContext = await AuthenticationHelper.GetAppOnlySharePointClientContextAsync())
            {
                var web = clientContext.Site.RootWeb;
                var list = web.Lists.GetByTitle(SPLists.MetricIdeas.Title);

                var item = list.AddItem(new ListItemCreationInformation());
                if (metricId.HasValue)
                    item[SPLists.MetricIdeas.Columns.Metric] = new FieldLookupValue { LookupId = metricId.Value };
                item[SPLists.MetricIdeas.Columns.TaskId] = task.Id;
                item[SPLists.MetricIdeas.Columns.TaskName] = task.Title;
                item[SPLists.MetricIdeas.Columns.TaskURL] = new FieldUrlValue { Url = taskURL, Description = taskURL };
                item[SPLists.MetricIdeas.Columns.TaskStartDate] = task.StartDateTime;
                item[SPLists.MetricIdeas.Columns.InputDate] = task.CreatedDateTime;
                item[SPLists.MetricIdeas.Columns.TaskStatus] = bucket;
                item.Update();

                clientContext.ExecuteQuery();
            }
        }

        private Metric ToMetric(ListItem item)
        {
            return new Metric
            {
                Id = item[SPLists.Issues.Columns.ID] as int?,
                Name = item[SPLists.Metrics.Columns.Title] as string
            };
        }
    }
}
