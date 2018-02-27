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
    public class ReasonValuesService
    {
        public static async Task<List<ReasonValue[]>> GetItemsAsync(List<string> reasonIds, DateTime weekStartDate)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var startUTC = weekStartDate.ToISO8601DateTimeString();
                var endUTC = weekStartDate.AddDays(7).ToISO8601DateTimeString();
                var reasonIdQuery =
                    @"<In>
                        <FieldRef Name='{0}' LookupId='TRUE'/>
                        <Values>"
                        + string.Join("", reasonIds.Select(readId => { return @"<Value Type='Lookup'>" + readId + "</Value>"; }))
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
                    SPLists.ReasonValues.Columns.Reason, SPLists.ReasonValues.Columns.Date, startUTC, endUTC);

                var query = new CamlQuery();
                query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

                var items = clientContext.GetItems(SPLists.ReasonValues.Title, query);
                return items.Select(item => item.ToReasonValue())
                    .GroupBy(item => item.Reason.Id)
                    .Select(grp => grp.ToArray())
                    .ToList();
            }
        }

        public static async Task InsertItemAsync(ReasonValue item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var list = clientContext.Web.Lists.GetByTitle(SPLists.ReasonValues.Title); ;
                ListItemCreationInformation newItem = new ListItemCreationInformation();
                ListItem listItem = list.AddItem(new ListItemCreationInformation());
                listItem[SPLists.ReasonValues.Columns.Reason] = SharePointHelper.BuildSingleLookFieldValue(item.Reason.Id, item.Reason.Name);
                listItem[SPLists.ReasonValues.Columns.Date] = item.InputDate;
                listItem[SPLists.ReasonValues.Columns.Value] = item.Value;
                listItem.Update();
                clientContext.Load(listItem);
                clientContext.ExecuteQuery();
                item.Id = listItem.Id;
            }
        }

        public static async Task UpdateItemAsync(ReasonValue item)
        {
            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var query = new CamlQuery();
                query.ViewXml =
                    @"<View>
                        <Query>
                            <Where>
                                <Eq>
                                    <FieldRef Name='" + SPLists.ReasonValues.Columns.ID + @"'/>
                                    <Value Type='int'>" + item.Id + @"</Value>
                                </Eq>
                            </Where>
                         </Query>
                    </View>";
                var items = clientContext.GetItems(SPLists.ReasonValues.Title, query);
                var queryItem = items.FirstOrDefault();
                if (queryItem == null) return;

                queryItem[SPLists.ReasonValues.Columns.Value] = item.Value;
                queryItem.Update();
                clientContext.ExecuteQuery();
            }
        }

        public static async Task DeleteReasonValuesByReasonId(int reasonId)
        {
            var filter = string.Format(
                @"<Eq>
                    <FieldRef Name='{0}' LookupId='TRUE'/>
                    <Value Type='Lookup'>{1}</Value> 
                </Eq>",
                SPLists.ReasonValues.Columns.Reason, reasonId);

            var query = new CamlQuery();
            query.ViewXml = string.Format("<View><Query><Where>{0}</Where></Query></View>", filter);

            using (var clientContext = await (AuthenticationHelper.GetSharePointClientContextAsync(Permissions.Application)))
            {
                var items = clientContext.GetItems(SPLists.ReasonValues.Title, query);
                for (int i = items.Count - 1; i > -1; i--)
                {
                    items[i].DeleteObject();
                    clientContext.ExecuteQuery();
                }
            }
        }
    }
}
