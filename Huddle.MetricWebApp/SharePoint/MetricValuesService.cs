/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using Huddle.Common;
using Huddle.MetricWebApp.Infrastructure;
using Huddle.MetricWebApp.Models;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Huddle.MetricWebApp.SharePoint
{
    public class MetricValuesService
    {
        public static async Task<List<MetricValue[]>> GetItemsAsync(List<string> metricIds, DateTime weekStartDate)
        {
            if (!metricIds.Any())
                return new List<MetricValue[]>();

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var startUTC = weekStartDate.ToISO8601DateTimeString();
                var endUTC = weekStartDate.AddDays(7).ToISO8601DateTimeString();
                var reasonIdQuery =
                    @"<In>
                        <FieldRef Name='{0}' LookupId='TRUE'/>
                        <Values>"
                        + string.Join("", metricIds.Select(metricId => @"<Value Type='Lookup'>" + metricId + "</Value>"))
                        + @"</Values>
                    </In>";
                var filter = string.Format(
                    @"<And>"
                        + reasonIdQuery
                        + @"<And>
                            <Geq>
                                <FieldRef Name='{1}' />
                                <Value IncludeTimeValue='TRUE' StorageTZ='TRUE' Type='DateTime'>{2}</Value>
                            </Geq>
                            <Leq>
                                <FieldRef Name='{1}' />
                                <Value IncludeTimeValue='TRUE' StorageTZ='TRUE' Type='DateTime'>{3}</Value>
                            </Leq>
                        </And>
                    </And>",
                    SPLists.MetricValuess.Columns.Metric, SPLists.MetricValuess.Columns.Date, startUTC, endUTC);

                var query = new CamlQuery();
                query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

                var items = clientContext.GetItems(SPLists.MetricValuess.Title, query);
                return items.Select(item => item.ToMetricValue())
                    .GroupBy(item => item.Metric.Id)
                    .Select(grp => grp.ToArray())
                    .ToList();
            }
        }

        public static async Task InsertItemAsync(MetricValue item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var list = clientContext.Web.Lists.GetByTitle(SPLists.MetricValuess.Title);
                ListItemCreationInformation newItem = new ListItemCreationInformation();
                ListItem listItem = list.AddItem(new ListItemCreationInformation());
                listItem[SPLists.MetricValuess.Columns.Metric] = SharePointHelper.BuildSingleLookFieldValue(item.Metric.Id, item.Metric.Name);
                listItem[SPLists.MetricValuess.Columns.Date] = item.InputDate;
                listItem[SPLists.MetricValuess.Columns.Value] = item.Value;
                listItem.Update();
                clientContext.Load(listItem);
                clientContext.ExecuteQuery();
                item.Id = listItem.Id;
            }
        }

        public static async Task UpdateItemAsync(MetricValue item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.MetricValuess.Columns.ID + @"'/>
                                    <Value Type='int'>" + item.Id + @"</Value>
                                </Eq>
                            </Where>
                         </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.MetricValuess.Title, query);
                var queryItem = items.FirstOrDefault();
                if (queryItem == null) return;

                queryItem[SPLists.MetricValuess.Columns.Value] = item.Value;
                queryItem.Update();
                clientContext.ExecuteQuery();
            }
        }

        public static async Task DeleteMetricValuesBMetricId(int metricId)
        {
            var filter = string.Format(@"
                    <Eq>
                        <FieldRef Name='{0}' LookupId='TRUE'/>
                        <Value Type='Lookup'>{1}</Value> 
                    </Eq>",
                    SPLists.MetricValuess.Columns.Metric, metricId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.MetricValuess.Title, query);

                for (int i = items.Count - 1; i > -1; i--)
                {
                    items[i].DeleteObject();
                    clientContext.ExecuteQuery();
                }
            }
        }
    }
}
